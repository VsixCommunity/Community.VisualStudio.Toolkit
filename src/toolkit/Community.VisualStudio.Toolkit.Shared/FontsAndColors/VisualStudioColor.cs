using System.Windows.Media;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines a color used by Visual Studio.
    /// </summary>
    public class VisualStudioColor
    {
        private bool _automatic;
        private COLORINDEX? _index;
        private Color? _rgb;
        private int? _sysColor;
        private int? _vsColor;

        private VisualStudioColor() { }

        /// <summary>
        /// Creates the color for the "Automatic" color.
        /// </summary>
        /// <returns>The color for Visual Studio.</returns>
        public static VisualStudioColor Automatic()
        {
            return new VisualStudioColor { _automatic = true };
        }

        /// <summary>
        /// Creates a color from the given predefined color value.
        /// </summary>
        /// <param name="index">The predefined color value.</param>
        /// <returns>The color for Visual Studio.</returns>
        public static VisualStudioColor Indexed(COLORINDEX index)
        {
            return new VisualStudioColor { _index = index };
        }

        /// <summary>
        /// Creates a color from the given red, green and blue components.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <returns>The color for Visual Studio.</returns>
        public static VisualStudioColor Rgb(byte r, byte g, byte b)
        {
            return Rgb(Color.FromRgb(r, g, b));
        }

        /// <summary>
        /// Creates a color from the given <see cref="Color"/> object.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The color for Visual Studio.</returns>
        public static VisualStudioColor Rgb(Color color)
        {
            return new VisualStudioColor { _rgb = color };
        }

        /// <summary>
        /// Creates a color that is defined by operating system.
        /// </summary>
        /// <param name="sysColor">The identifier of the color.</param>
        /// <returns>The color for Visual Studio.</returns>
        public static VisualStudioColor SysColor(int sysColor)
        {
            return new VisualStudioColor { _sysColor = sysColor };
        }

        /// <summary>
        /// Creates a color that is defined by Visual Studio.
        /// </summary>
        /// <param name="color">The identifier of the color.</param>
        /// <returns>The color for Visual Studio.</returns>
        public static VisualStudioColor VsColor(__VSSYSCOLOREX color)
        {
            return new VisualStudioColor { _vsColor = (int)color };
        }

        /// <summary>
        /// Creates a color that is defined by Visual Studio.
        /// </summary>
        /// <param name="color">The identifier of the color.</param>
        /// <returns>The color for Visual Studio.</returns>
        public static VisualStudioColor VsColor(__VSSYSCOLOREX2 color)
        {
            return new VisualStudioColor { _vsColor = (int)color };
        }

        /// <summary>
        /// Creates a color that is defined by Visual Studio.
        /// </summary>
        /// <param name="color">The identifier of the color.</param>
        /// <returns>The color for Visual Studio.</returns>
        public static VisualStudioColor VsColor(__VSSYSCOLOREX3 color)
        {
            return new VisualStudioColor { _vsColor = (int)color };
        }

        internal uint ToColorRef(IVsFontAndColorUtilities utilities)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            uint result;

            if (_automatic)
            {
                ErrorHandler.ThrowOnFailure(utilities.EncodeAutomaticColor(out result));
                return result;
            }

            if (_index.HasValue)
            {
                ErrorHandler.ThrowOnFailure(utilities.EncodeIndexedColor(_index.Value, out result));
                return result;
            }

            if (_rgb.HasValue)
            {
                return (uint)(_rgb.Value.R | (_rgb.Value.G << 8) | (_rgb.Value.B << 16));
            }

            if (_sysColor.HasValue)
            {
                ErrorHandler.ThrowOnFailure(utilities.EncodeSysColor(_sysColor.Value, out result));
                return result;
            }

            if (_vsColor.HasValue)
            {
                ErrorHandler.ThrowOnFailure(utilities.EncodeVSColor(_vsColor.Value, out result));
                return result;
            }

            ErrorHandler.ThrowOnFailure(utilities.EncodeInvalidColor(out result));
            return result;
        }
    }
}
