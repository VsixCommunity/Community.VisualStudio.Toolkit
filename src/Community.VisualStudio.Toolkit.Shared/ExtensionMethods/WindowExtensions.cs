using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit.Shared.ExtensionMethods
{
    /// <summary>Extension methods for the <see cref="Window"/> class.</summary>
    public static class WindowExtensions
    {
        /// <summary>Shows a window as a dialog.</summary>
        public static async Task<bool?> ShowDialogAsync(this Window window, WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterOwner)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var vsUiShellService = await VS.GetServiceAsync<SVsUIShell, IVsUIShell>();
            Assumes.Present(vsUiShellService);

            ErrorHandler.ThrowOnFailure(vsUiShellService.GetDialogOwnerHwnd(out var hwnd));
            ErrorHandler.ThrowOnFailure(vsUiShellService.EnableModeless(0));

            window.WindowStartupLocation = windowStartupLocation;

            try
            {
                WindowHelper.ShowModal(window, hwnd);
                return window.DialogResult;
            }
            finally
            {
                vsUiShellService.EnableModeless(1);
            }
        }
    }
}
