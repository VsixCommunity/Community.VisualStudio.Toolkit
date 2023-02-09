using System;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines font styles.
    /// </summary>
    /// <remarks>
    /// Equivalent to <see cref="FONTFLAGS"/> and <see cref="__FCFONTFLAGS"/>.
    /// </remarks>
    [Flags]
    public enum FontStyle
    {
        /// <summary>
        /// Plain text.
        /// </summary>
        None = 0,
        /// <summary>
        /// Bold text.
        /// </summary>
        Bold = FONTFLAGS.FF_BOLD,
        /// <summary>
        /// Strikethrough text.
        /// </summary>
        Strikethrough = FONTFLAGS.FF_STRIKETHROUGH,
        /// <summary>
        /// Specifies that the "bold" attribute of this Display Item will be the same as the "bold" attribute of the "plain text" item.
        /// </summary>
        TrackPlaintextBold = __FCFONTFLAGS.FCFF_TRACK_PLAINTEXT_BOLD
    }
}
