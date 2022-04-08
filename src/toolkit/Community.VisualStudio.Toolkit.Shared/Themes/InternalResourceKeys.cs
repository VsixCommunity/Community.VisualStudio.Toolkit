using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Re-defines resource keys and other properties used in XAML files, where the assembly that 
    /// they are originally defined in has a different name depending on the version of the toolkit.
    /// </summary>
    internal static class InternalResourceKeys
    {
        // Defined in "clr-namespace:Microsoft.VisualStudio.Shell;assembly=Microsoft.VisualStudio.Shell.xx.0"
        public static object VsResourceKeys_TextBoxStyleKey => VsResourceKeys.TextBoxStyleKey;
        public static object VsResourceKeys_ComboBoxStyleKey => VsResourceKeys.ComboBoxStyleKey;

        // Defined in "clr-namespace:Microsoft.VisualStudio.PlatformUI;assembly=Microsoft.VisualStudio.Shell.xx.0"
        public static object CommonControlsColors_TextBoxBackgroundBrushKey => CommonControlsColors.TextBoxBackgroundBrushKey;
        public static object CommonControlsColors_TextBoxTextBrushKey => CommonControlsColors.TextBoxTextBrushKey;
        public static object CommonControlsColors_TextBoxBorderBrushKey => CommonControlsColors.TextBoxBorderBrushKey;
        public static object CommonControlsColors_TextBoxBackgroundFocusedBrushKey => CommonControlsColors.TextBoxBackgroundFocusedBrushKey;
        public static object CommonControlsColors_TextBoxTextFocusedBrushKey => CommonControlsColors.TextBoxTextFocusedBrushKey;
        public static object CommonControlsColors_TextBoxBorderFocusedBrushKey => CommonControlsColors.TextBoxBorderFocusedBrushKey;
        public static object CommonControlsColors_TextBoxBackgroundDisabledBrushKey => CommonControlsColors.TextBoxBackgroundDisabledBrushKey;
        public static object CommonControlsColors_TextBoxTextDisabledBrushKey => CommonControlsColors.TextBoxTextDisabledBrushKey;
        public static object CommonControlsColors_TextBoxBorderDisabledBrushKey => CommonControlsColors.TextBoxBorderDisabledBrushKey;
    }
}
