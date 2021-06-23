using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
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

        /// <summary>Provides methods to manage the global designer verbs and menu commands available in design mode, and to show some types of shortcut menus.</summary>
        public Task<IMenuCommandService> GetCommandServiceAsync() => VS.GetRequiredServiceAsync<IMenuCommandService, IMenuCommandService>();

        /// <summary>Used to register and unregister a command target as a high priority command handler.</summary>
        public Task<IVsRegisterPriorityCommandTarget> GetPriorityCommandTargetAsync() => VS.GetRequiredServiceAsync<SVsRegisterPriorityCommandTarget, IVsRegisterPriorityCommandTarget>();

        /// <summary>
        /// Executes a command by name
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public async Task<bool> ExecuteAsync(string name, string argument = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsCommandWindow cw = await VS.Windows.GetCommandWindowAsync();

            return cw.ExecuteCommand($"{name} {argument}") == VSConstants.S_OK;
        }

        /// <summary>
        /// Executes a command by guid and ID
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public async Task<bool> ExecuteAsync(Guid menuGroup, uint commandId, string argument = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IOleCommandTarget cs = await VS.GetRequiredServiceAsync<SUIHostCommandDispatcher, IOleCommandTarget>();
            
            IntPtr inArgPtr = Marshal.AllocCoTaskMem(200);
            Marshal.GetNativeVariantForObject(argument, inArgPtr);

            var result = cs.Exec(menuGroup, commandId, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, inArgPtr, IntPtr.Zero);

            return result == VSConstants.S_OK;
        }

        /// <summary>
        /// Executes a command from the <see cref="VSConstants.VSStd97CmdID"/> collection of built in commands.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(VSConstants.VSStd97CmdID command, string argument = "")
        {
            return ExecuteAsync(typeof(VSConstants.VSStd97CmdID).GUID, (uint)command, argument);
        }

        /// <summary>
        /// Executes a command from the <see cref="VSConstants.VSStd2KCmdID"/> collection of built in commands.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public Task<bool> ExecuteAsync(VSConstants.VSStd2KCmdID command, string argument = "")
        {
            return ExecuteAsync(typeof(VSConstants.VSStd2KCmdID).GUID, (uint)command, argument);
        }
    }
}
