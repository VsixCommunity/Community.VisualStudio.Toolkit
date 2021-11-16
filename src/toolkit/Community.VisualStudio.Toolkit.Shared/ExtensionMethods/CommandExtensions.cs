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
            int hr = VSConstants.E_FAIL;

            if (await cmd.IsAvailableAsync())
            {
                IOleCommandTarget cs = await VS.GetRequiredServiceAsync<SUIHostCommandDispatcher, IOleCommandTarget>();

                int argByteCount = Encoding.Unicode.GetByteCount(argument);
                IntPtr inArgPtr = Marshal.AllocCoTaskMem(argByteCount);

                try
                {
                    Marshal.GetNativeVariantForObject(argument, inArgPtr);
                    hr = cs.Exec(cmd.Guid, (uint)cmd.ID, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, inArgPtr, IntPtr.Zero);
                }
                finally
                {
                    Marshal.Release(inArgPtr);
                }
            }

            return hr == VSConstants.S_OK;
        }

        /// <summary>
        /// Checks if a command is enabled and supported.
        /// </summary>
        public static async Task<bool> IsAvailableAsync(this CommandID cmd)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IOleCommandTarget cs = await VS.GetRequiredServiceAsync<SUIHostCommandDispatcher, IOleCommandTarget>();

            try
            {
                Guid guid = cmd.Guid;
                OLECMD[] prgCmds = new OLECMD[1];
                prgCmds[0].cmdID = (uint)cmd.ID;
                int hr = cs.QueryStatus(ref guid, (uint)cmd.ID, prgCmds, IntPtr.Zero);

                if (ErrorHandler.Succeeded(hr))
                {
                    if ((prgCmds[0].cmdf & (uint)OLECMDF.OLECMDF_ENABLED) != 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }

            return false;
        }
    }
}
