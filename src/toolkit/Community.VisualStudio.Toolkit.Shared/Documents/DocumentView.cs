using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents an open text document view.
    /// </summary>
    public class DocumentView
    {
        internal DocumentView(WindowFrame? frame, IWpfTextView? view)
        {
            WindowFrame = frame;
            TextView = view;
            Document = TextBuffer?.GetTextDocument();
            FilePath = Document?.FilePath;
        }

        internal DocumentView(IVsWindowFrame nativeFrame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            WindowFrame frame = new(nativeFrame);
            IWpfTextView? view = VsShellUtilities.GetTextView(nativeFrame)?.ToIWpfTextView();
            
            WindowFrame = frame;
            TextView = view;
            Document = TextBuffer?.GetTextDocument();
            nativeFrame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out object pvar);

            if (pvar is string filePath)
            {
                FilePath = filePath;
            }
        }

        /// <summary>
        /// The window frame hosting the document.
        /// </summary>
        public WindowFrame? WindowFrame { get; }

        /// <summary>
        /// The text view loaded in the window frame.
        /// </summary>
        public IWpfTextView? TextView { get; }

        /// <summary>
        /// The text document loaded in the view.
        /// </summary>
        public ITextDocument? Document { get; }

        /// <summary>
        /// The text buffer loaded in the view.
        /// </summary>
        public ITextBuffer? TextBuffer => TextView?.TextBuffer;

        /// <summary>
        /// The absolute file path of the document.
        /// </summary>
        public string? FilePath { get; }
    }
}
