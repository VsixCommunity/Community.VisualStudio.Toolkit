using System.Runtime.InteropServices;
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
            if (!await cmd.IsAvailableAsync())
            {
                return false;
            }

            IOleCommandTarget cs = await VS.GetRequiredServiceAsync<SUIHostCommandDispatcher, IOleCommandTarget>();

            IntPtr vIn = IntPtr.Zero;
            Guid g = cmd.Guid;

            try
            {
                if (argument != null)
                {
                    vIn = Marshal.AllocCoTaskMem(128);
                    Marshal.GetNativeVariantForObject(argument, vIn);
                }

                int hr = cs.Exec(ref g, unchecked((uint)cmd.ID), (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, vIn, IntPtr.Zero);
                return ErrorHandler.Succeeded(hr);
            }
            finally
            {
                if (vIn != IntPtr.Zero)
                {
                    NativeMethods.VariantClear(vIn);
                    Marshal.FreeCoTaskMem(vIn);
                }
            }
        }

        /// <summary>
        /// Checks if a command is enabled and supported.
        /// </summary>
        public static async Task<bool> IsAvailableAsync(this CommandID cmd)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IOleCommandTarget cs = await VS.GetRequiredServiceAsync<SUIHostCommandDispatcher, IOleCommandTarget>();

            Guid guid = cmd.Guid;
            OLECMD[]? cmds = new OLECMD[1];
            cmds[0].cmdID = (uint)cmd.ID;
            cmds[0].cmdf = 0;

            int hr = cs.QueryStatus(ref guid, (uint)cmds.Length, cmds, IntPtr.Zero);

            if (ErrorHandler.Succeeded(hr))
            {
                if (((OLECMDF)cmds[0].cmdf).HasFlag(OLECMDF.OLECMDF_ENABLED))
                {
                    return true;
                }
            }

            return false;
        }

        private static class NativeMethods
        {
            [DllImport("oleaut32.dll")]
            public static extern int VariantClear(IntPtr v);

            [DllImport("oleaut32.dll")]
            public static extern int VariantInit(IntPtr v);
        }
    }
}
