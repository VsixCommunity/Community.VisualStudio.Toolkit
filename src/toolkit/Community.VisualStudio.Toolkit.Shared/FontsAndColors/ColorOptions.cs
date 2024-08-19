using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines what a user can change about a <see cref="ColorDefinition"/>.
    /// </summary>
    /// <remarks>Equivalent to <see cref="__FCITEMFLAGS"/>.</remarks>
    [Flags]
    public enum ColorOptions
    {
        /// <summary>
        /// No special behavior.
        /// </summary>
        None = 0,
        /// <summary>
        /// Enables the Background Color drop-down box that allows the user to change the background color.
        /// </summary>
        AllowBackgroundChange = __FCITEMFLAGS.FCIF_ALLOWBGCHANGE,
        /// <summary>
        /// Enables the Bold check box that allows the user to change the bold attribute.
        /// </summary>
        AllowBoldChange = __FCITEMFLAGS.FCIF_ALLOWBOLDCHANGE,
        /// <summary>
        /// Enables the Custom buttons that allows the user to create and select customized colors.
        /// </summary>
        AllowCustomColors = __FCITEMFLAGS.FCIF_ALLOWCUSTOMCOLORS,
        /// <summary>
        /// Enables the Foreground Color drop-down box that allows the user to change the foreground color.
        /// </summary>
        AllowForegroundChange = __FCITEMFLAGS.FCIF_ALLOWFGCHANGE,
        /// <summary>
        /// Specifies that the item is a marker type.
        /// </summary>
        IsMarker = __FCITEMFLAGS.FCIF_ISMARKER,
        /// <summary>
        /// Indicates that the Display Items is to be treated as "plain text." This means that the color used to display the item will track the environment wide font and color settings for plain text color.
        /// </summary>
        PlainText = __FCITEMFLAGS.FCIF_PLAINTEXT,
    }
}
