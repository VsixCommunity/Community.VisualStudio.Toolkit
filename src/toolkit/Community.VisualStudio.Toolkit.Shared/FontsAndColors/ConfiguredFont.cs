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
        private double _size;
        private byte _characterSet;

        internal ConfiguredFont(ref FontInfo info)
        {
            _familyName = info.bstrFaceName;
            _size = info.wPointSize;
            _characterSet = info.iCharSet;
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
        /// The font size.
        /// </summary>
        public double Size => _size;

        /// <summary>
        /// The character set.
        /// </summary>
        public byte CharacterSet => _characterSet;

        internal bool Update(ref FontInfo info)
        {
            bool changed = false;
            string oldFaceName = _familyName;
            double oldSize = _size;
            byte oldCharacterSet = _characterSet;

            // Update all of the fields first so that
            // everything is set before we raise the events.
            _familyName = info.bstrFaceName;
            _size = info.wPointSize;
            _characterSet = info.iCharSet;

            if (!string.Equals(oldFaceName, _familyName))
            {
                changed = true;
                _family = null;
                _hasFamily = false;
                NotifyPropertyChanged(nameof(Family));
                NotifyPropertyChanged(nameof(FamilyName));
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
    }
}
