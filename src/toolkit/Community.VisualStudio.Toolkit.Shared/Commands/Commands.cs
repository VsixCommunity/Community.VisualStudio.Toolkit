using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to the command system.</summary>
    public class Commands
    {
        internal Commands()
        { }

        /// <summary>
        /// Finds a command by cannonical name.
        /// </summary>
        public async Task<CommandID?> FindCommandAsync(string name)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsCmdNameMapping cmdNameMap = await VS.Services.GetCommandNameMappingAsync();
            int hr = cmdNameMap.MapNameToGUIDID(name, out Guid commandGroup, out uint commandId);

            if (hr == VSConstants.S_OK)
            {
                return new CommandID(commandGroup, (int)commandId);
            }

            return null;
        }

        /// <summary>
        /// Executes a command by name
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public async Task<bool> ExecuteAsync(string name, string argument = "")
        {
            CommandID? cmd = await FindCommandAsync(name);
            if (cmd == null)
            {
                return false;
            }

            return await cmd.ExecuteAsync(argument);
        }

        /// <summary>
        /// Checks if a command is enabled and supported.
        /// </summary>
        public async Task<bool> IsAvailableAsync(string name)
        {
            CommandID? cmd = await FindCommandAsync(name);
            if (cmd == null)
            {
                return false;
            }

            return await cmd.IsAvailableAsync();
        }

        /// <summary>
        /// Executes a command by GUID and ID
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(Guid menuGroup, int commandId, string argument = "")
           => ExecuteAsync(new CommandID(menuGroup, commandId), argument);

        /// <summary>
        /// Executes a command from the <see cref="VSConstants.VSStd97CmdID"/> collection of built in commands.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(VSConstants.VSStd97CmdID command, string argument = "")
          => ExecuteAsync(typeof(VSConstants.VSStd97CmdID).GUID, (int)command, argument);

        /// <summary>
        /// Executes a command from the <see cref="VSConstants.VSStd2KCmdID"/> collection of built in commands.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(VSConstants.VSStd2KCmdID command, string argument = "")
          => ExecuteAsync(typeof(VSConstants.VSStd2KCmdID).GUID, (int)command, argument);

        /// <summary>
        /// Executes a command by GUID and ID
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(CommandID cmd, string argument = "")
          => cmd.ExecuteAsync(argument);

        /// <summary>
        /// Intercept any command before it is being handled by other command handlers.
        /// </summary>
        /// <returns>Returns an <see cref="IDisposable"/> that will remove the command interceptor when disposed.</returns>
        public Task<IDisposable> InterceptAsync(VSConstants.VSStd97CmdID command, Func<CommandProgression> func)
            => InterceptAsync(typeof(VSConstants.VSStd97CmdID).GUID, (int)command, func);

        /// <summary>
        /// Intercept any command before it is being handled by other command handlers.
        /// </summary>
        /// <returns>Returns an <see cref="IDisposable"/> that will remove the command interceptor when disposed.</returns>
        public Task<IDisposable> InterceptAsync(VSConstants.VSStd2KCmdID command, Func<CommandProgression> func)
            => InterceptAsync(typeof(VSConstants.VSStd2KCmdID).GUID, (int)command, func);

        /// <summary>
        /// Intercept any command before it is being handled by other command handlers.
        /// </summary>
        /// <returns>Returns an <see cref="IDisposable"/> that will remove the command interceptor when disposed.</returns>
        public Task<IDisposable> InterceptAsync(Guid menuGroup, int commandId, Func<CommandProgression> func)
            => InterceptAsync(new CommandID(menuGroup, commandId), func);

        /// <summary>
        /// Intercept any command before it is being handled by other command handlers.
        /// </summary>
        /// <returns>Returns an <see cref="IDisposable"/> that will remove the command interceptor when disposed.</returns>
        public async Task<IDisposable> InterceptAsync(CommandID cmd, Func<CommandProgression> func)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsRegisterPriorityCommandTarget? priority = await VS.Services.GetPriorityCommandTargetAsync();
            CommandInterceptor interceptor = new(cmd, func);

            ErrorHandler.ThrowOnFailure(priority.RegisterPriorityCommandTarget(0, interceptor, out uint cookie));

            return new Disposable(() => priority.UnregisterPriorityCommandTarget(cookie));
        }
    }

    internal class CommandInterceptor : IOleCommandTarget
    {
        private readonly CommandID _cmd;
        private readonly Func<CommandProgression> _func;

        public CommandInterceptor(CommandID cmd, Func<CommandProgression> func)
        {
            _cmd = cmd;
            _func = func;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (prgCmds[0].cmdID == _cmd.ID)
            {
                prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_ENABLED | (uint)OLECMDF.OLECMDF_SUPPORTED;
                return VSConstants.S_OK;
            }
            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == _cmd.Guid && nCmdID == _cmd.ID)
            {
                if (_func() == CommandProgression.Stop)
                {
                    // This will stop the chain of command handlers from progressing.
                    return VSConstants.S_OK;
                }
            }

            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.MSOCMDERR_E_FIRST;
        }
    }

    /// <summary>
    /// Holds values on how the command execution should proceed.
    /// </summary>
    public enum CommandProgression
    {
        /// <summary>Proceed to execute the next command handler for the command.</summary>
        Continue,
        /// <summary>Stop execution and don't continue execution to the next command handler.</summary>
        Stop,
    }
}
