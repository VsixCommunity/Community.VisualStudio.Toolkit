using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Services related to the selection of windows and items in solution.
    /// </summary>
    public class Selection
    {
        internal Selection()
        { }

        /// <summary>
        /// Sets the current UI context.
        /// </summary>
        /// <param name="uiContextGuid">The GUID to uniquely identify the UI context.</param>
        /// <param name="isActive">Determines if the UI context is active or not.</param>
        public Task SetUIContextAsync(string uiContextGuid, bool isActive) 
            => SetUIContextAsync(new Guid(uiContextGuid), isActive);

        /// <summary>
        /// Sets the current UI context.
        /// </summary>
        /// <param name="uiContextGuid">The GUID to uniquely identify the UI context.</param>
        /// <param name="isActive">Determines if the UI context is active or not.</param>
        public async Task SetUIContextAsync(Guid uiContextGuid, bool isActive)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection svc = await VS.Services.GetMonitorSelectionAsync();

            int cookieResult = svc.GetCmdUIContextCookie(uiContextGuid, out uint cookie);
            ErrorHandler.ThrowOnFailure(cookieResult);

            int setContextResult = svc.SetCmdUIContext(cookie, isActive ? 1 : 0);
            ErrorHandler.ThrowOnFailure(setContextResult);
        }
    }
}
