using System;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Creates InfoBar controls for use on documents and tool windows.
    /// </summary>
    public class InfoBarFactory
    {
        /// <summary>
        /// Creates a new InfoBar in any tool- or document window.
        /// </summary>
        /// <param name="windowGuidOrFileName">The GUID of the tool window or filename of document. For instance, <c>ToolWindowGuids80.SolutionExplorer</c></param>
        /// <param name="model">A model representing the text, icon, and actions of the InfoBar.</param>
        public InfoBar CreateInfoBar(string windowGuidOrFileName, InfoBarModel model)
        {
            return new InfoBar(windowGuidOrFileName, model);
        }
    }

    /// <summary>
    /// An instance of an InfoBar (also known as Yellow- or Gold bar).
    /// </summary>
    public class InfoBar : IVsInfoBarUIEvents
    {
        private readonly string _windowIdentifier;
        private readonly InfoBarModel _model;
        private IVsInfoBarUIElement? _uiElement;

        /// <summary>
        /// Creates a new instance of the InfoBar in a specific window frame or document window.
        /// </summary>
        internal InfoBar(string windowIdentifier, InfoBarModel model)
        {
            _windowIdentifier = windowIdentifier;
            _model = model;
        }

        /// <summary>
        /// Indicates if the InfoBar is visible in the UI or not.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Displays the InfoBar in the tool window or document previously specified.
        /// </summary>
        /// <returns><c>true</c> if the InfoBar was shown; otherwise <c>false</c>.</returns>
        public async Task<bool> TryShowInfoBarUIAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var infoBarUIFactory = (IVsInfoBarUIFactory)await VS.GetRequiredServiceAsync<SVsInfoBarUIFactory, object>();

            _uiElement = infoBarUIFactory.CreateInfoBar(_model);
            _uiElement.Advise(this, out _);

            IVsInfoBarHost? host = await GetInfoBarHostAsync();

            if (host != null)
            {
                host.AddInfoBar(_uiElement);
                IsVisible = true;
            }

            return IsVisible;
        }

        /// <summary>
        /// Closes the InfoBar without the user manually had to do it.
        /// </summary>
        public void Close()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (IsVisible && _uiElement != null)
            {
                _uiElement.Close();
            }
        }

        /// <summary>
        /// An event triggered when an action item in the InfoBar is clicked.
        /// </summary>
        public event EventHandler<InfoBarActionItemEventArgs>? ActionItemClicked;

        private async Task<IVsInfoBarHost?> GetInfoBarHostAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsWindowFrame? frame;

            // Tool Window
            if (Guid.TryParse(_windowIdentifier, out Guid guid))
            {
                IVsUIShell? uiShell = await VS.Services.GetUIShellAsync();
                Assumes.Present(uiShell);
                uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref guid, out frame);
            }

            // Document window
            else if (VsShellUtilities.IsDocumentOpen(ServiceProvider.GlobalProvider, _windowIdentifier, Guid.Empty, out _, out _, out frame))
            {
                // Do nothing, the 'frame' is assigned
            }

            if (frame != null)
            {
                frame.GetProperty((int)__VSFPROPID7.VSFPROPID_InfoBarHost, out var host);
                return host as IVsInfoBarHost;
            }

            return null;
        }

        void IVsInfoBarUIEvents.OnClosed(IVsInfoBarUIElement infoBarUIElement)
        {
            IsVisible = false;
        }

        void IVsInfoBarUIEvents.OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
            ActionItemClicked?.Invoke(this, new InfoBarActionItemEventArgs(infoBarUIElement, _model, actionItem));
        }
    }
}
