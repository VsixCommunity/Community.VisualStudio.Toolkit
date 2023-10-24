using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Details of a category's color that may have been changed by the user. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// This object's properties will be updated when the user changes the 
    /// color settings on the <i>Fonts and Colors</i> options page.
    /// </para>
    /// <para>
    /// This class implements <see cref="INotifyPropertyChanged"/>, which allows it to be used in XAML bindings.
    /// </para>
    /// </remarks>
    public class ConfiguredColor : ObservableObject
    {
        private Color _backgroundColor;
        private Color _foregroundColor;
        private FontStyle _fontStyle;
        private SolidColorBrush? _backgroundBrush;
        private SolidColorBrush? _foregroundBrush;
        private FontWeight _fontWeight;

        internal ConfiguredColor(uint background, uint foreground, FontStyle fontStyle)
        {
            _backgroundColor = ToColor(background);
            _foregroundColor = ToColor(foreground);
            _fontStyle = fontStyle;
            _fontWeight = FontWeight.FromOpenTypeWeight(GetFontWeightValue(fontStyle));
        }

        /// <summary>
        /// The background color.
        /// </summary>
        public Color BackgroundColor => _backgroundColor;

        /// <summary>
        /// A brush using <see cref="BackgroundColor"/>
        /// </summary>
        public Brush BackgroundBrush => _backgroundBrush ??= new SolidColorBrush(BackgroundColor);

        /// <summary>
        /// The foreground color.
        /// </summary>
        public Color ForegroundColor => _foregroundColor;

        /// <summary>
        /// A brush using <see cref="ForegroundColor"/>.
        /// </summary>
        public Brush ForegroundBrush => _foregroundBrush ??= new SolidColorBrush(ForegroundColor);

        /// <summary>
        /// The font style.
        /// </summary>
        public FontStyle FontStyle => _fontStyle;

        /// <summary>
        /// The font weight.
        /// </summary>
        public FontWeight FontWeight => _fontWeight;

        internal bool Update(uint background, uint foreground, FontStyle fontStyle)
        {
            bool changed = false;
            Color oldBackgroundColor = _backgroundColor;
            Color oldForegroundColor = _foregroundColor;
            FontStyle oldFontStyle = _fontStyle;
            FontWeight oldFontWeight = _fontWeight;

            // Update all of the fields first so that
            // everything is set before we raise the events.
            _backgroundColor = ToColor(background);
            _foregroundColor = ToColor(foreground);
            _fontStyle = fontStyle;
            _fontWeight = FontWeight.FromOpenTypeWeight(GetFontWeightValue(fontStyle));

            // Clear the brushes and font weight if their underlying value has changed.
            bool backgroundChanged = !oldBackgroundColor.Equals(_backgroundColor);
            bool foregroundChanged = !oldForegroundColor.Equals(_foregroundColor);

            if (backgroundChanged)
            {
                _backgroundBrush = null;
            }

            if (foregroundChanged)
            {
                _foregroundBrush = null;
            }

            // Now that everything has been set, we can raise the events.
            if (backgroundChanged)
            {
                changed = true;
                NotifyPropertyChanged(nameof(BackgroundColor));
                NotifyPropertyChanged(nameof(BackgroundBrush));
            }

            if (foregroundChanged)
            {
                changed = true;
                NotifyPropertyChanged(nameof(ForegroundColor));
                NotifyPropertyChanged(nameof(ForegroundBrush));
            }

            if (oldFontStyle != _fontStyle)
            {
                changed = true;
                NotifyPropertyChanged(nameof(FontStyle));
            }

            if (oldFontWeight.ToOpenTypeWeight() != _fontWeight.ToOpenTypeWeight())
            {
                changed = true;
                NotifyPropertyChanged(nameof(FontWeight));
            }

            return changed;
        }

        private static Color ToColor(uint color)
        {
            return Color.FromRgb(
                (byte)(color & 0xff),
                (byte)((color & 0xff00) >> 8),
                (byte)((color & 0xff0000) >> 16)
            );
        }

        private static int GetFontWeightValue(FontStyle style)
        {
            if ((style & FontStyle.Bold) == FontStyle.Bold)
            {
                return 700;
            }
            else
            {
                return 400;
            }
        }
    }
}
