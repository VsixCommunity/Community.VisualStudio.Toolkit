using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A category's font and colors that may have been changed by the user.
    /// </summary>
    public sealed class ConfiguredFontAndColorSet<T> : IDisposable, IFontAndColorChangeListener where T : BaseFontAndColorCategory<T>, new()
    {
        private readonly Dictionary<ColorDefinition, ConfiguredColor> _colors;
        private readonly Action<IFontAndColorChangeListener> _onDispose;

        internal ConfiguredFontAndColorSet(
            T category,
            ref FontInfo font,
            Dictionary<ColorDefinition, ConfiguredColor> colors,
            Action<IFontAndColorChangeListener> onDispose
        )
        {
            Category = category;
            Font = new ConfiguredFont(ref font);
            _colors = colors;
            _onDispose = onDispose;
        }

        /// <summary>
        /// The category that this font and color set is associated with.
        /// </summary>
        public T Category { get; }

        /// <summary>
        /// The font details.
        /// </summary>
        public ConfiguredFont Font { get; }

        /// <summary>
        /// Gets the <see cref="ConfiguredColor"/> that corresponds to the given <see cref="ColorDefinition"/>.
        /// </summary>
        /// <param name="definition">The color definition to get the color for.</param>
        /// <returns>The configured color.</returns>
        /// <exception cref="ArgumentException">
        /// The given definition does not belong to the category that this font and color set is associated with.
        /// </exception>
        public ConfiguredColor GetColor(ColorDefinition definition)
        {
            if (!_colors.TryGetValue(definition, out ConfiguredColor color))
            {
                throw new ArgumentException(
                    $"The color definition '{definition.Name}' does not belong to the category '{Category.Name}'."
                );
            }
            return color;
        }

        /// <summary>
        /// Raised when the configured font is changed.
        /// </summary>
        public event EventHandler? FontChanged;

        /// <summary>
        /// Raised when a configured color is changed.
        /// </summary>
        public event EventHandler<ConfiguredColorChangedEventArgs>? ColorChanged;

        void IFontAndColorChangeListener.SetFont(ref FontInfo info)
        {
            if (Font.Update(ref info))
            {
                FontChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        void IFontAndColorChangeListener.SetColor(ColorDefinition definition, uint background, uint foreground, FontStyle fontStyle)
        {
            if (_colors.TryGetValue(definition, out ConfiguredColor? color))
            {
                if (color.Update(background, foreground, fontStyle))
                {
                    ColorChanged?.Invoke(this, new ConfiguredColorChangedEventArgs(definition, color));
                }
            }
        }

        /// <summary>
        /// Steps the <see cref="ConfiguredFont"/> and <see cref="ConfiguredColor"/> objects provided by this class
        /// from being updated when the user changes the font or color settings on the <i>Fonts and Colors</i> options page.
        /// </summary>
        public void Dispose()
        {
            _onDispose.Invoke(this);
        }
    }
}
