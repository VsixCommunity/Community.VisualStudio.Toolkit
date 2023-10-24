using System;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Arguments for the event that is raised when a configured color changes.
    /// </summary>
    public class ConfiguredColorChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredColorChangedEventArgs"/> class.
        /// </summary>
        /// <param name="definition">The definition of the color that was changed.</param>
        /// <param name="color">The color that was changed.</param>
        public ConfiguredColorChangedEventArgs(ColorDefinition definition, ConfiguredColor color)
        {
            Definition = definition;
            Color = color;
        }

        /// <summary>
        /// The definition of the color that was changed.
        /// </summary>
        public ColorDefinition Definition { get; }

        /// <summary>
        /// The color that was changed.
        /// </summary>
        public ConfiguredColor Color { get; }

    }
}
