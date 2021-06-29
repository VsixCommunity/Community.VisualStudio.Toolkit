using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to the command system.</summary>
    public class Commands
    {
        internal Commands()
        { }

        /// <summary>Provides methods to manage the global designer verbs and menu commands available in design mode, and to show some types of shortcut menus.</summary>
        public Task<IMenuCommandService> GetCommandServiceAsync() => VS.GetRequiredServiceAsync<IMenuCommandService, IMenuCommandService>();

        /// <summary>Used to register and unregister a command target as a high priority command handler.</summary>
        public Task<IVsRegisterPriorityCommandTarget> GetPriorityCommandTargetAsync() => VS.GetRequiredServiceAsync<SVsRegisterPriorityCommandTarget, IVsRegisterPriorityCommandTarget>();

        /// <summary>
        /// Finds a command by cannonical name.
        /// </summary>
        public async Task<CommandID?> FindCommandAsync(string name)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsCommandWindow cw = await VS.Windows.GetCommandWindowAsync();

            var hr = cw.PrepareCommand(name, out Guid commandGroup, out var commandId, out _, new PREPARECOMMANDRESULT[0]);

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

            if (cmd != null)
            {
                return await ExecuteAsync(cmd.Guid, cmd.ID, argument);
            }

            return false;
        }

        /// <summary>
        /// Executes a command by guid and ID
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(Guid menuGroup, int commandId, string argument = "")
        {
            return ExecuteAsync(new CommandID(menuGroup, commandId), argument);
        }

        /// <summary>
        /// Executes a command by guid and ID
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(CommandID cmd, string argument = "")
        {
            return cmd.ExecuteAsync(argument);
        }

        /// <summary>
        /// Executes a command from the <see cref="VSConstants.VSStd97CmdID"/> collection of built in commands.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(VSConstants.VSStd97CmdID command, string argument = "")
        {
            return ExecuteAsync(typeof(VSConstants.VSStd97CmdID).GUID, (int)command, argument);
        }

        /// <summary>
        /// Executes a command from the <see cref="VSConstants.VSStd2KCmdID"/> collection of built in commands.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(VSConstants.VSStd2KCmdID command, string argument = "")
        {
            return ExecuteAsync(typeof(VSConstants.VSStd2KCmdID).GUID, (int)command, argument);
        }
    }
}
