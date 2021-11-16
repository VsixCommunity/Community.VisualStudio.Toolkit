using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace System.ComponentModel.Design
{
    /// <summary>
    /// Extension methods for <see cref="System.Threading.Tasks.Task" /> and <see cref="JoinableTask" />.
    /// </summary>
    public static class CommandExtensions
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command was succesfully executed; otherwise <see langword="false"/>.</returns>
        public static async Task<bool> ExecuteAsync(this CommandID cmd, string argument = "")
        {
            try
            {
                IVsUIShell uiShell = await VS.Services.GetUIShellAsync();
                Guid commandGuid = cmd.Guid;

                int hr = uiShell.PostExecCommand(ref commandGuid, unchecked((uint)cmd.ID), 0, argument);

                return hr == VSConstants.S_OK;
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
                return false;
            }
        }
    }
}
