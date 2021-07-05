using System;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Text
{
    /// <summary>
    /// Extension methods for the ITextBuffer interface.
    /// </summary>
    public static class ITextBufferExtensions
    {
        /// <summary>
        /// Opens an undo context with the specified name. Remember to call <c>Complete()</c> and <c>Dispose()</c> to commit the transaction.
        /// </summary>
        public static async Task<ITextUndoTransaction> OpenUndoContextAsync(this ITextBuffer buffer, string name)
        {
            ITextUndoHistoryRegistry? registry = await VS.GetMefServiceAsync<ITextUndoHistoryRegistry>();
            return registry.GetHistory(buffer).CreateTransaction(name);
        }

        /// <summary>
        /// Gets the text document.
        /// </summary>
        public static ITextDocument? GetTextDocument(this ITextBuffer buffer)
        {
            if (buffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out ITextDocument? document))
            {
                return document;
            }

            return null;
        }

        /// <summary>
        /// Gets the file name on disk associated with the buffer.
        /// </summary>
        public static string? GetFileName(this ITextBuffer buffer)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!buffer.Properties.TryGetProperty(typeof(IVsTextBuffer), out IVsTextBuffer bufferAdapter))
            {
                return null;
            }

            string? ppzsFilename = null;
            int returnCode = -1;

            if (bufferAdapter is IPersistFileFormat persistFileFormat)
            {
                try
                {
                    returnCode = persistFileFormat.GetCurFile(out ppzsFilename, out uint pnFormatIndex);
                }
                catch (NotImplementedException)
                {
                    return null;
                }
            }

            if (returnCode != VSConstants.S_OK)
            {
                return null;
            }

            return ppzsFilename;
        }
    }
}
