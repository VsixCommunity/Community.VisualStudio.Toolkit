using System;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Events related to the selection in Visusal Studio.
    /// </summary>
    public class DebuggerEvents : IVsDebuggerEvents
    {
        internal DebuggerEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var svc = (IVsDebugger)ServiceProvider.GlobalProvider.GetService(typeof(IVsDebugger));
            Assumes.Present(svc);
            svc.AdviseDebuggerEvents(this, out _);
        }

        /// <summary>
        /// Fires when entering break mode.
        /// </summary>
        public event EventHandler? EnterBreakMode;

        /// <summary>
        /// Fired when the debugger enters run mode.
        /// </summary>
        public event EventHandler? EnterRunMode;

        /// <summary>
        /// Fired when leaving run mode or debug mode, and when the debugger establishes design mode after debugging.
        /// </summary>
        public event EventHandler? EnterDesignMode;

        /// <summary>
        /// Fires when entering Edit &amp; Continue mode.
        /// </summary>
        public event EventHandler? EnterEditAndContinueMode;

        int IVsDebuggerEvents.OnModeChange(DBGMODE dbgmodeNew)
        {
            switch (dbgmodeNew)
            {
                case DBGMODE.DBGMODE_Design:
                    EnterDesignMode?.Invoke(this, EventArgs.Empty);
                    break;
                case DBGMODE.DBGMODE_Break:
                    EnterBreakMode?.Invoke(this, EventArgs.Empty);
                    break;
                case DBGMODE.DBGMODE_Run:
                    EnterRunMode?.Invoke(this, EventArgs.Empty);
                    break;
                case DBGMODE.DBGMODE_Enc:
                    EnterEditAndContinueMode?.Invoke(this, EventArgs.Empty);
                    break;
            }

            return VSConstants.S_OK;
        }
    }
}
