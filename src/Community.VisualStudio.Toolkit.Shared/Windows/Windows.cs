using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to windows.</summary>
    public class Windows
    {
        internal Windows()
        { }

        /// <summary>
        /// Output window panes provided by Visual Studio.
        /// </summary>
        public enum VSOutputWindowPane
        {
            /// <summary>The General pane.</summary>
            General,
            /// <summary>The Build pane.</summary>
            Build,
            /// <summary>The Debug pane.</summary>
            Debug,
            /// <summary>The sorted build output pane inside the output window.</summary>
            SortedBuild
        }

        /// <summary>
        /// Creates a new Output window pane with the given name.
        /// The pane can be created now or lazily upon the first write to it.
        /// </summary>
        /// <param name="name">The name (title) of the new pane.</param>
        /// <param name="lazyCreate">Whether to lazily create the pane upon first write.</param>
        /// <returns>A new OutputWindowPane.</returns>
        public Task<OutputWindowPane> CreateOutputWindowPaneAsync(string name, bool lazyCreate = true)
            => OutputWindowPane.CreateAsync(name, lazyCreate);

        /// <summary>
        /// Gets an existing Visual Studio Output window pane (General, Build, Debug).
        /// If the General pane does not already exist then it will be created, but that is not the case
        /// for Build or Debug, in which case the method returns null if the pane doesn't already exist.
        /// </summary>
        /// <param name="pane">The Visual Studio pane to get.</param>
        /// <returns>A new OutputWindowPane or null.</returns>
        public Task<OutputWindowPane?> GetOutputWindowPaneAsync(VSOutputWindowPane pane)
            => OutputWindowPane.GetAsync(pane);

        /// <summary>
        /// Gets an existing Output window pane.
        /// Returns null if a pane with the specified GUID does not exist.
        /// </summary>
        /// <param name="guid">The pane's unique identifier.</param>
        /// <returns>A new OutputWindowPane or <see langword="null"/>.</returns>
        public Task<OutputWindowPane?> GetOutputWindowPaneAsync(Guid guid)
            => OutputWindowPane.GetAsync(guid);

        /// <summary>Shows a window as a dialog.</summary>
        public async Task<bool?> ShowDialogAsync(Window window, WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterOwner)
            => await window.ShowDialogAsync(windowStartupLocation);

        /// <summary>
        /// Gets the current active window frame object.
        /// </summary>
        public async Task<WindowFrame?> GetCurrentWindowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection? svc = await VS.Services.GetMonitorSelectionAsync();
            svc.GetCurrentElementValue((uint)VSConstants.VSSELELEMID.SEID_WindowFrame, out object selection);

            if (selection is IVsWindowFrame frame)
            {
                return new WindowFrame(frame);
            }

            return null;
        }

        /// <summary>
        /// Find the open window frame hosting the specified file.
        /// </summary>
        /// <returns><see langword="null"/> if the file isn't open.</returns>
        public async Task<WindowFrame?> FindDocumentWindowAsync(string file)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.IsDocumentOpen(ServiceProvider.GlobalProvider, file, Guid.Empty, out _, out _, out IVsWindowFrame? frame);

            if (frame != null)
            {
                return new WindowFrame(frame);
            }

            return null;
        }

        /// <summary>
        /// Finds tool windows matching the specified GUID.
        /// </summary>
        /// <param name="toolWindowGuid">Find known tool window GUIDs in the <see cref="WindowGuids"/> class.</param>
        /// <returns>An instance of an <see cref="IVsWindowFrame"/> or <see langword="null"/>.</returns>
        public async Task<WindowFrame?> FindWindowAsync(Guid toolWindowGuid)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsUIShell uiShell = await VS.Services.GetUIShellAsync();
            int hr = uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref toolWindowGuid, out IVsWindowFrame? frame);

            if (hr == VSConstants.S_OK)
            {
                return new WindowFrame(frame);
            }

            return null;
        }

        /// <summary>
        /// Finds tool windows matching the specified GUID.
        /// </summary>
        /// <param name="toolWindowGuid">Find known tool window GUIDs in the <see cref="WindowGuids"/> class.</param>
        /// <returns>An instance of an <see cref="IVsWindowFrame"/> or <see langword="null"/>.</returns>
        public async Task<WindowFrame?> FindOrShowToolWindowAsync(Guid toolWindowGuid)
        {
            return await FindWindowAsync(toolWindowGuid) ?? await ShowToolWindowAsync(toolWindowGuid);
        }

        /// <summary>
        /// Shows any toolwindow.
        /// </summary>
        /// <param name="toolWindowGuid">Find known tool window GUIDs in the <see cref="WindowGuids"/> class.</param>
        /// <returns>An instance of an <see cref="IVsWindowFrame"/> or <see langword="null"/>.</returns>
        public async Task<WindowFrame?> ShowToolWindowAsync(Guid toolWindowGuid)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsUIShell uiShell = await VS.Services.GetUIShellAsync();
            int hr = uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref toolWindowGuid, out IVsWindowFrame? frame);

            if (hr == VSConstants.S_OK)
            {
                frame.Show();
                return new WindowFrame(frame);
            }

            return null;
        }

        /// <summary>
        /// Obtains all window frames visible in the IDE.
        /// </summary>
        /// <value>All available window frames.</value>
        public async Task<IEnumerable<WindowFrame>> GetAllWindowsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsUIShell uiShell = await VS.Services.GetUIShellAsync();

            ErrorHandler.ThrowOnFailure(uiShell.GetToolWindowEnum(out IEnumWindowFrames windowEnumerator));
            IVsWindowFrame[] frame = new IVsWindowFrame[1];
            int hr = VSConstants.S_OK;
            List<WindowFrame> list = new();

            while (hr == VSConstants.S_OK)
            {
                hr = windowEnumerator.Next(1, frame, out uint fetched);
                ErrorHandler.ThrowOnFailure(hr);

                if (fetched == 1)
                {
                    list.Add(new WindowFrame(frame[0]));
                }
            }

            return list;
        }
    }
}