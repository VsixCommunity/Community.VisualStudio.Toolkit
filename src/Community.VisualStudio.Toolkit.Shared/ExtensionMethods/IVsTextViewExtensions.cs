using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.ComponentModelHost;
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
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return textView.ToDocumentView();
        }

        /// <summary>
        /// Converts an <see cref="IVsTextView"/> to a <see cref="DocumentView"/>.
        /// </summary>
        /// <returns><see langword="null"/> if the textView is null or the conversion failed.</returns>
        internal static DocumentView ToDocumentView(this IVsTextView textView)
        {
            if (textView == null || textView is not IVsTextViewEx nativeView)
            {
                return new DocumentView(null, null);
            }

            ErrorHandler.ThrowOnFailure(nativeView.GetWindowFrame(out object frameValue));

            ThreadHelper.ThrowIfNotOnUIThread();

            IWpfTextView? view = ToIWpfTextView(textView);

            if (frameValue is IVsWindowFrame frame && view != null)
            {
                WindowFrame windowFrame = new WindowFrame(frame);
                return new DocumentView(windowFrame, view);
            }

            return new DocumentView(null, view);
        }

        /// <summary>
        /// Converts an <see cref="IWpfTextView"/> to a <see cref="DocumentView"/>.
        /// </summary>
        /// <returns><see langword="null"/> if the textView is null or the conversion failed.</returns>
        public static async Task<DocumentView?> ToDocumentViewAsync(this IWpfTextView textView)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return textView.ToDocumentView();
        }

        /// <summary>
        /// Converts an <see cref="IWpfTextView"/> to a <see cref="DocumentView"/>.
        /// </summary>
        /// <returns><see langword="null"/> if the textView is null or the conversion failed.</returns>
        internal static DocumentView ToDocumentView(this IWpfTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsTextView? nativeView = textView.ToIVsTextView();

            if (nativeView != null)
            {
                return nativeView.ToDocumentView();
            }

            return new DocumentView(null, textView);
        }

        /// <summary>
        /// Converts the <see cref="IVsTextView"/> to an <see cref="IWpfTextView"/>/
        /// </summary>
        /// <returns></returns>
        public static async Task<IWpfTextView?> ToIWpfTextViewAsync(this IVsTextView nativeView)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return nativeView.ToIWpfTextView();
        }

        /// <summary>
        /// Converts the <see cref="IVsTextView"/> to an <see cref="IWpfTextView"/>/
        /// </summary>
        /// <returns></returns>
        internal static IWpfTextView? ToIWpfTextView(this IVsTextView nativeView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsEditorAdaptersFactoryService? editorAdapter = VS.GetMefService<IVsEditorAdaptersFactoryService>();
            return editorAdapter?.GetWpfTextView(nativeView);
        }

        /// <summary>
        /// Converts the <see cref="IVsTextView"/> to an <see cref="IWpfTextView"/>/
        /// </summary>
        /// <returns></returns>
        public static async Task<IVsTextView?> ToIVsTextViewAsync(this IWpfTextView view)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return view.ToIVsTextView();
        }

        /// <summary>
        /// Converts the <see cref="IVsTextView"/> to an <see cref="IWpfTextView"/>/
        /// </summary>
        /// <returns></returns>
        internal static IVsTextView? ToIVsTextView(this IWpfTextView view)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsEditorAdaptersFactoryService? editorAdapter = VS.GetMefService<IVsEditorAdaptersFactoryService>();
            return editorAdapter?.GetViewAdapter(view);
        }
    }
}
