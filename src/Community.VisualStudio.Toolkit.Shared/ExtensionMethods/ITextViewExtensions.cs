using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Text
{
    /// <summary>
    /// Extension methods for <see cref="ITextView" />.
    /// </summary>
    public static class ITextViewExtensions
    {
        /// <summary>
        /// Creates an instance of an <see cref="InfoBar"/> in the text view.
        /// </summary>
        public static async Task<InfoBar?> CreateInfoBarAsync(this ITextView textView, InfoBarModel model)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var fileName = textView.TextBuffer.GetFileName();

            if (!string.IsNullOrEmpty(fileName))
            {
                return VS.InfoBar.CreateInfoBar(fileName!, model);
            }

            return null;
        }
    }
}
