using System;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Events related to the Visual Studio Shell.
    /// </summary>
    public class ShellEvents : IVsShellPropertyEvents, IVsBroadcastMessageEvents
    {
        private const uint _wM_SYSCOLORCHANGE = 0x0015;

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

            return VSConstants.S_OK;
        }

        int IVsBroadcastMessageEvents.OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == _wM_SYSCOLORCHANGE)
            {
                EnvironmentColorChanged?.Invoke();  
            }

            return VSConstants.S_OK;
        }
    }
}
