using System;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    public partial class Events
    {
        /// <summary>
        /// Events related to the Visual Studio Shell.
        /// </summary>
        public ShellEvents ShellEvents => new();
    }

    /// <summary>
    /// Events related to the Visual Studio Shell.
    /// </summary>
    public class ShellEvents : IVsShellPropertyEvents, IVsBroadcastMessageEvents
    {
        private const uint _wm_syscolorchange = 0x0015;

        internal ShellEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var svc = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            Assumes.Present(svc);
            svc.AdviseShellPropertyChanges(this, out _);
            svc.AdviseBroadcastMessages(this, out _);
        }

        /// <summary>
        /// When Visual Studio enters into an interactive state.
        /// </summary>
        public event Action? ShellAvailable;

        /// <summary>
        /// When Visual Studio starts to shutdown
        /// </summary>
        public event Action? ShutdownStarted;

        /// <summary>
        /// When Visual Studio starts to shutdown
        /// </summary>
        public event Action<bool>? MainWindowVisibilityChanged;

        /// <summary>
        /// When Visual Studio enters into an interactive state.
        /// </summary>
        public event Action? EnvironmentColorChanged;

        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
        {
            if (propid == (int)__VSSPROPID.VSSPROPID_Zombie || propid == (int)__VSSPROPID4.VSSPROPID_ShellInitialized)
            {
                if (!(bool)var)
                {
                    ShellAvailable?.Invoke();
                }
            }
            else if (propid == (int)__VSSPROPID6.VSSPROPID_ShutdownStarted)
            {
                if (!(bool)var)
                {
                    ShutdownStarted?.Invoke();
                }
            }
            else if (propid == (int)__VSSPROPID2.VSSPROPID_MainWindowVisibility)
            {
                // TODO: Test to see if 'var' is a bool. It may be an int
                if (var is bool isVisible)
                {
                    MainWindowVisibilityChanged?.Invoke(isVisible);
                }
            }
            
            return VSConstants.S_OK;
        }

        int IVsBroadcastMessageEvents.OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == _wm_syscolorchange)
            {
                EnvironmentColorChanged?.Invoke();  
            }

            return VSConstants.S_OK;
        }
    }
}
