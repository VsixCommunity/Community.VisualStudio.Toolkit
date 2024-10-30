using System;
using System.ComponentModel;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A category's font that may have been changed by the user.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This object's properties will be updated when the user changes the 
    /// font settings on the <i>Fonts and Colors</i> options page.
    /// </para>
    /// <para>
    /// This class implements <see cref="INotifyPropertyChanged"/>, which allows it to be used in XAML bindings.
    /// </para>
    /// </remarks>
    public class ConfiguredFont : ObservableObject
    {
        private FontFamily? _family;
        private bool _hasFamily;
        private string _familyName;
        private int _pointSize;
        private double _size;
        private byte _characterSet;

        internal ConfiguredFont(ref LOGFONTW logfont, ref FontInfo fontInfo)
        {
            _familyName = fontInfo.bstrFaceName;
            _pointSize = fontInfo.wPointSize;
            _size = CalculateFontSize(ref logfont);
            _characterSet = fontInfo.iCharSet;
        }

        /// <summary>
        /// The font family. This will be <see langword="null"/> when the font family name is "Automatic".
        /// </summary>
        public FontFamily? Family
        {
            get
            {
                if (!_hasFamily)
                {
                    if (string.Equals(_familyName, FontDefinition.Automatic.FamilyName))
                    {
                        _family = null;
                    }
                    else
                    {
                        _family = new FontFamily(_familyName);
                    }
                    _hasFamily = true;
                }
                return _family;
            }
        }

        /// <summary>
        /// The font family name.
        /// </summary>
        public string FamilyName => _familyName;

        /// <summary>
        /// The font size, in points. This is the value specified on the <i>Fonts and Colors</i> options page.
        /// </summary>
        public int PointSize => _pointSize;

        /// <summary>
        /// The font size, for use in WPF. This is the font size that can be used in WPF controls.
        /// For example, the value can be used directly in the 
        /// <see cref="System.Windows.Documents.TextElement.FontSize"/> property.
        /// </summary>
        public double Size => _size;

        /// <summary>
        /// The character set.
        /// </summary>
        public byte CharacterSet => _characterSet;

        internal bool Update(ref LOGFONTW logfont, ref FontInfo info)
        {
            bool changed = false;
            string oldFaceName = _familyName;
            int oldPointSize = _pointSize;
            double oldSize = _size;
            byte oldCharacterSet = _characterSet;

            // Update all of the fields first so that
            // everything is set before we raise the events.
            _familyName = info.bstrFaceName;
            _pointSize = info.wPointSize;
            _size = CalculateFontSize(ref logfont);
            _characterSet = info.iCharSet;

            if (!string.Equals(oldFaceName, _familyName))
            {
                changed = true;
                _family = null;
                _hasFamily = false;
                NotifyPropertyChanged(nameof(Family));
                NotifyPropertyChanged(nameof(FamilyName));
            }

            if (oldPointSize != _pointSize)
            {
                changed = true;
                NotifyPropertyChanged(nameof(PointSize));
            }

            if (oldSize != _size)
            {
                changed = true;
                NotifyPropertyChanged(nameof(Size));
            }

            if (oldCharacterSet != _characterSet)
            {
                changed = true;
                NotifyPropertyChanged(nameof(CharacterSet));
            }

            return changed;
        }

        private static double CalculateFontSize(ref LOGFONTW logfont)
        {
            return Math.Abs(logfont.lfHeight) * 96.0 /
#if VS14
                // `DpiAwareness` does not exist in VS 14, so default
                // to 96.0, which is the standard system DPI.
                96.0
#else
                Microsoft.VisualStudio.Utilities.DpiAwareness.SystemDpiY
#endif
                ;
        }
    }
}
