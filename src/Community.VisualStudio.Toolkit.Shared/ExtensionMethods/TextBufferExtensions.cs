using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.Text
{
    /// <summary>
    /// Extension methods for the ITextBuffer interface.
    /// </summary>
    public static class TextBufferExtensions
    {
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
            var returnCode = -1;

            if (bufferAdapter is IPersistFileFormat persistFileFormat)
            {
                try
                {
                    returnCode = persistFileFormat.GetCurFile(out ppzsFilename, out var pnFormatIndex);
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
