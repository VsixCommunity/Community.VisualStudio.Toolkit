using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
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
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IOleCommandTarget cs = await VS.GetRequiredServiceAsync<SUIHostCommandDispatcher, IOleCommandTarget>();

            int argByteCount = Encoding.Unicode.GetByteCount(argument);
            IntPtr inArgPtr = Marshal.AllocCoTaskMem(argByteCount);

            try
            {
                Marshal.GetNativeVariantForObject(argument, inArgPtr);
                int result = cs.Exec(cmd.Guid, (uint)cmd.ID, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, inArgPtr, IntPtr.Zero);

                return result == VSConstants.S_OK;
            }
            finally
            {
                Marshal.Release(inArgPtr);
            }
        }
    }
}
