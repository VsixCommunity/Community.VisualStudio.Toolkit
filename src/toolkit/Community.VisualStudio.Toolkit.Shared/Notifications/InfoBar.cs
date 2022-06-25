using System;
using System.Threading.Tasks;
using System.Windows.Controls;
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
        /// Creates a new InfoBar in the main window.
        /// </summary>
        /// <param name="model">A model representing the text, icon, and actions of the InfoBar.</param>
        public async Task<InfoBar?> CreateAsync(InfoBarModel model)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsShell shell = await VS.Services.GetShellAsync();
            shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out object value);

            if (value is IVsInfoBarHost host)
            {
                return new InfoBar(host, model);
            }

            return null;
        }

        /// <summary>
        /// Creates a new InfoBar in any tool- or document window.
        /// </summary>
        /// <param name="windowGuidOrFileName">The GUID of the tool window or filename of document. For instance, <c>ToolWindowGuids80.SolutionExplorer</c></param>
        /// <param name="model">A model representing the text, icon, and actions of the InfoBar.</param>
        public async Task<InfoBar?> CreateAsync(string windowGuidOrFileName, InfoBarModel model)
        {
            IVsWindowFrame? frame = await GetFrameFromIdentifierAsync(windowGuidOrFileName);

            if (frame != null)
            {
                return await CreateAsync(frame, model);
            }

            return null;
        }

        /// <summary>
        /// Creates a new InfoBar in any tool- or document window.
        /// </summary>
        public async Task<InfoBar?> CreateAsync(IVsWindowFrame frame, InfoBarModel model)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            frame.GetProperty((int)__VSFPROPID7.VSFPROPID_InfoBarHost, out object value);

            if (value is IVsInfoBarHost host)
            {
                return new InfoBar(host, model);
            }

            return null;
        }

        private async Task<IVsWindowFrame?> GetFrameFromIdentifierAsync(string identifier)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsWindowFrame? frame;

            // Tool Window
            if (Guid.TryParse(identifier, out Guid guid))
            {
                IVsUIShell uiShell = await VS.Services.GetUIShellAsync();
                uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref guid, out frame);
            }

            // Document window
            else if (VsShellUtilities.IsDocumentOpen(ServiceProvider.GlobalProvider, identifier, Guid.Empty, out _, out _, out frame))
            {
                // Do nothing, the 'frame' is assigned
            }

            return frame;
        }
    }

    /// <summary>
    /// An instance of an InfoBar (also known as Yellow- or Gold bar).
    /// </summary>
    public class InfoBar : IVsInfoBarUIEvents
    {
        private readonly IVsInfoBarHost _host;
        private readonly InfoBarModel _model;
        private IVsInfoBarUIElement? _uiElement;
        private uint _listenerCookie;

        /// <summary>
        /// Creates a new instance of the InfoBar in a specific window frame or document window.
        /// </summary>
        internal InfoBar(IVsInfoBarHost host, InfoBarModel model)
        {
            _host = host;
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
            IVsInfoBarUIFactory infoBarUIFactory = (IVsInfoBarUIFactory)await VS.GetRequiredServiceAsync<SVsInfoBarUIFactory, object>();

            _uiElement = infoBarUIFactory.CreateInfoBar(_model);
            _uiElement.Advise(this, out _listenerCookie);

            if (_host != null)
            {
                _host.AddInfoBar(_uiElement);
                IsVisible = true;
            }

            return IsVisible;
        }

        /// <summary>
        /// Attempts to get the underlying WPF UI element of the InfoBar
        /// </summary>
        public bool TryGetWpfElement(out Control? control)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            object? uiObject = null;
            control = null;
            _uiElement?.GetUIObject(out uiObject);

            if (uiObject is IVsUIWpfElement elm)
            {
                elm.GetFrameworkElement(out object frameworkElement);
                control = frameworkElement as Control;
            }

            return control != null;
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

        void IVsInfoBarUIEvents.OnClosed(IVsInfoBarUIElement infoBarUIElement)
        {
            IsVisible = false;
            _uiElement?.Unadvise(_listenerCookie);
        }

        void IVsInfoBarUIEvents.OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
        {
            ActionItemClicked?.Invoke(this, new InfoBarActionItemEventArgs(infoBarUIElement, _model, actionItem));
        }
    }
}
