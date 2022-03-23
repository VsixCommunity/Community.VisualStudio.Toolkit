using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Handles debugging.
    /// </summary>
    public class Debugger
    {
        /// <summary>
        /// The mode of the debugger.
        /// </summary>
        public enum DebugMode
        {
            /// <summary>The debugger is not attached.</summary>
            NotDebugging,
            /// <summary>The debugger is stopped at a breakpoint.</summary>
            AtBreakpoint,
            /// <summary>The debugger is attached and running.</summary>
            Running
        }

        /// <summary>
        /// Checks if the debugger is attached.
        /// </summary>
        public async Task<bool> IsDebuggingAsync()
        {
            DebugMode debugMode = await GetDebugModeAsync();
            return debugMode != DebugMode.NotDebugging;
        }

        /// <summary>
        /// Returns the current mode for the debugger.
        /// </summary>
        public async Task<DebugMode> GetDebugModeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsDebugger debugger = await VS.Services.GetDebuggerAsync();
            DBGMODE[] mode = new DBGMODE[1];
            ErrorHandler.ThrowOnFailure(debugger.GetMode(mode));
            DBGMODE dbgMode = mode[0] & ~DBGMODE.DBGMODE_EncMask;

            return dbgMode switch
            {
                DBGMODE.DBGMODE_Design => DebugMode.NotDebugging,
                DBGMODE.DBGMODE_Break => DebugMode.AtBreakpoint,
                DBGMODE.DBGMODE_Run => DebugMode.Running,
                _ => throw new InvalidOperationException($"Unexpected {nameof(DebugMode)}")
            };
        }
    }
}
