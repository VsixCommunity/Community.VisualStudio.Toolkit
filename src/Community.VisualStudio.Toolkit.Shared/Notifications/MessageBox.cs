using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static Microsoft.VisualStudio.VSConstants;

namespace Community.VisualStudio.Toolkit
{
    public partial class Notifications
    {
        /// <summary>
        /// Shows a message box that is parented to the main Visual Studio window
        /// </summary>
        /// <returns>
        /// The result of which button on the message box was clicked.
        /// </returns>
        /// <example>
        /// <code>
        /// VS.Notifications.ShowMessage("Title", "The message");
        /// </code>
        /// </example>
        public MessageBoxResult ShowMessage(string title,
                                            string message,
                                            OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO,
                                            OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL,
                                            OLEMSGDEFBUTTON defaultButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var result = VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, message, title, icon, buttons, defaultButton);

            return (MessageBoxResult)result;
        }

        /// <summary>
        /// Shows an error message box.
        /// </summary>
        /// <returns>The result of which button on the message box was clicked.</returns>
        public MessageBoxResult ShowError(string title, string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ShowMessage(title, message, OLEMSGICON.OLEMSGICON_CRITICAL);
        }

        /// <summary>
        /// Shows a warning message box.
        /// </summary>
        /// <returns>The result of which button on the message box was clicked.</returns>
        public MessageBoxResult ShowWarning(string title, string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ShowMessage(title, message, OLEMSGICON.OLEMSGICON_WARNING);
        }

        /// <summary>
        /// Shows a yes/no/cancel message box .
        /// </summary>
        /// <returns>true if the user clicks the 'Yes' button.</returns>
        public bool ShowConfirm(string title, string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return ShowMessage(title, message, OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_YESNO) == MessageBoxResult.IDYES;
        }
    }
}
