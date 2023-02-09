using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Line style options.
    /// </summary>
    /// <remarks>Same as <see cref="LINESTYLE"/> and <see cref="LINESTYLE2"/>.</remarks>
    public enum LineStyle
    {
        /// <summary>
        /// No line.
        /// </summary>
        None = LINESTYLE.LI_NONE,
        /// <summary>
        /// Solid line.
        /// </summary>
        Solid = LINESTYLE.LI_SOLID,
        /// <summary>
        /// Squiggly line.
        /// </summary>
        Squiggly = LINESTYLE.LI_SQUIGGLY,
        /// <summary>
        /// Hatched pattern.
        /// </summary>
        Hatch = LINESTYLE.LI_HATCH,
        /// <summary>
        /// Fifty percent gray dither (dotted when 1 pixel).
        /// </summary>
        Dotted = LINESTYLE.LI_DOTTED,
        /// <summary>
        /// Smart tag factoid.
        /// </summary>
        SmartTagFactoid = LINESTYLE2.LI_SMARTTAGFACT,
        /// <summary>
        /// Smart tag factoid side.
        /// </summary>
        SmartTagFactoidSide = LINESTYLE2.LI_SMARTTAGFACTSIDE,
        /// <summary>
        /// Smart tag ephemeral.
        /// </summary>
        SmartTagEphemeral = LINESTYLE2.LI_SMARTTAGEPHEM,
        /// <summary>
        /// Smart tag ephemeral side.
        /// </summary>
        SmartTagEphemeralSide = LINESTYLE2.LI_SMARTTAGEPHEMSIDE
    }
}
