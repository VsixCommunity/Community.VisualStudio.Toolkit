using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    public partial class Events
    {
        private DebuggerEvents? _debuggerEvents;

        /// <summary>
        /// Events related to the debugger in Visual Studio.
        /// </summary>
        public DebuggerEvents DebuggerEvents => _debuggerEvents ??= new();
    }

    /// <summary>
    /// Events related to the debugger in Visual Studio.
    /// </summary>
    public class DebuggerEvents : IVsDebuggerEvents
    {
        internal DebuggerEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsDebugger svc = VS.GetRequiredService<IVsDebugger, IVsDebugger>();
            svc.AdviseDebuggerEvents(this, out _);
        }

        /// <summary>
        /// Fires when entering break mode.
        /// </summary>
        public event Action? EnterBreakMode;

        /// <summary>
        /// Fired when the debugger enters run mode.
        /// </summary>
        public event Action? EnterRunMode;

        /// <summary>
        /// Fired when leaving run mode or debug mode, and when the debugger establishes design mode after debugging.
        /// </summary>
        public event Action? EnterDesignMode;

        /// <summary>
        /// Fires when entering Edit &amp; Continue mode.
        /// </summary>
        public event Action? EnterEditAndContinueMode;

        int IVsDebuggerEvents.OnModeChange(DBGMODE dbgmodeNew)
        {
            switch (dbgmodeNew)
            {
                case DBGMODE.DBGMODE_Design:
                    EnterDesignMode?.Invoke();
                    break;
                case DBGMODE.DBGMODE_Break:
                    EnterBreakMode?.Invoke();
                    break;
                case DBGMODE.DBGMODE_Run:
                    EnterRunMode?.Invoke();
                    break;
                case DBGMODE.DBGMODE_Enc:
                    EnterEditAndContinueMode?.Invoke();
                    break;
            }

            return VSConstants.S_OK;
        }
    }
}
