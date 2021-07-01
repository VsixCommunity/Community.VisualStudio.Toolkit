using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.TextManager.Interop
{
    /// <summary>
    /// Extending the IVsTextView interface
    /// </summary>
    public static class IVsTextViewExtensions
    {
        /// <summary>
        /// Converts an <see cref="IVsTextView"/> to a <see cref="DocumentView"/>.
        /// </summary>
        /// <returns><see langword="null"/> if the textView is null or the conversion failed.</returns>
        public static async Task<DocumentView?> ToDocumentViewAsync(this IVsTextView textView)
        {
            if (textView == null || textView is not IVsTextViewEx nativeView)
            {
                return null;
            }

            ErrorHandler.ThrowOnFailure(nativeView.GetWindowFrame(out var frameValue));

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IWpfTextView? view = await ToIWpfTextViewAsync(textView);

            if (frameValue is IVsWindowFrame frame && view != null)
            {
                var windowFrame = new WindowFrame(frame);
                return new DocumentView(windowFrame, view);
            }

            return null;
        }

        /// <summary>
        /// Converts the <see cref="IVsTextView"/> to an <see cref="IWpfTextView"/>/
        /// </summary>
        /// <returns></returns>
        public static async Task<IWpfTextView?> ToIWpfTextViewAsync(IVsTextView nativeView)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsEditorAdaptersFactoryService? editorAdapter = await VS.GetMefServiceAsync<IVsEditorAdaptersFactoryService>();

            return editorAdapter?.GetWpfTextView(nativeView);
        }
    }
}
