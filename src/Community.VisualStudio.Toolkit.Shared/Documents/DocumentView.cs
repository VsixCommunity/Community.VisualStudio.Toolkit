using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

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
        }

        /// <summary>
        /// The window frame hosting the document.
        /// </summary>
        public WindowFrame? WindowFrame { get; }

        /// <summary>
        /// The text view loaded int he window frame.
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
    }
}
