using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines information about an item that can be changed on the <i>Fonts and Colors</i> page.
    /// </summary>
    /// <remarks>
    /// A <see cref="ColorDefinition"/> is immutable to prevent changes to it after it has been registered with Visual Studio.
    /// </remarks>
    /// <example>
    /// <code>
    /// public ColorDefinition Primary { get; } = new(
    ///     "Primary"
    ///     defaultBackground: VisualStudioColor.Indexed(COLORINDEX.CI_RED),
    ///     defaultForeground: VisualStudioColor.Indexed(COLORINDEX.CI_WHITE)
    /// );
    /// </code>
    /// </example>
    public class ColorDefinition
    {
        /// <summary>
        /// The default value of the <see cref="Options"/> property.
        /// </summary>
        public static readonly ColorOptions DefaultOptions =
            ColorOptions.AllowCustomColors | ColorOptions.AllowBackgroundChange | ColorOptions.AllowForegroundChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDefinition"/> class.
        /// </summary>
        /// <param name="name">The name of the color definition.</param>
        /// <param name="localizedName">The localized name of the color definition.</param>
        /// <param name="description">A description of what the color definition applies to.</param>
        /// <param name="lineStyle">The line style.</param>
        /// <param name="markerVisualStyle">The visual style when the color is used in a marker.</param>
        /// <param name="options">Determines what the user can customize about the color definition.</param>
        /// <param name="fontStyle">The font style.</param>
        /// <param name="defaultBackground">The default background color.</param>
        /// <param name="defaultForeground">The default foreground color.</param>
        /// <param name="automaticBackground">The color to use when "Automatic" is selected as the background color.</param>
        /// <param name="automaticForeground">The color to use when "Automatic" is selected as the foreground color.</param>
        public ColorDefinition(
            string name,
            string? localizedName = null,
            string? description = null,
            VisualStudioColor? defaultBackground = null,
            VisualStudioColor? defaultForeground = null,
            VisualStudioColor? automaticBackground = null,
            VisualStudioColor? automaticForeground = null,
            ColorOptions? options = null,
            FontStyle fontStyle = FontStyle.None,
            LineStyle lineStyle = LineStyle.None,
            MarkerVisualStyle markerVisualStyle = MarkerVisualStyle.None
        )
        {
            Name = name;
            LocalizedName = localizedName ?? name;
            Description = description ?? "";
            DefaultBackground = defaultBackground ?? VisualStudioColor.Automatic();
            DefaultForeground = defaultForeground ?? VisualStudioColor.Automatic();
            AutomaticBackground = automaticBackground ?? VisualStudioColor.VsColor(__VSSYSCOLOREX.VSCOLOR_ENVIRONMENT_BACKGROUND);
            AutomaticForeground = automaticForeground ?? VisualStudioColor.VsColor(__VSSYSCOLOREX.VSCOLOR_PANEL_TEXT);
            Options = options ?? DefaultOptions;
            FontStyle = fontStyle;
            LineStyle = lineStyle;
            MarkerVisualStyle = markerVisualStyle;
        }

        /// <summary>
        /// The name of the color definition shown on the <i>Fonts and Colors</i> page.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The localized name of this color definition.
        /// </summary>
        public string LocalizedName { get; }

        /// <summary>
        /// A description of what this color definition applies to.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The default background color.
        /// </summary>
        public VisualStudioColor DefaultBackground { get; }

        /// <summary>
        /// The default foreground color.
        /// </summary>
        public VisualStudioColor DefaultForeground { get; }

        /// <summary>
        /// The color to use when "Automatic" is selected as the background color.
        /// </summary>
        public VisualStudioColor AutomaticBackground { get; }

        /// <summary>
        /// The color to use when "Automatic" is selected as the foreground color.
        /// </summary>
        public VisualStudioColor AutomaticForeground { get; }

        /// <summary>
        /// Determines what the user can customize about this color definition.
        /// </summary>
        public ColorOptions Options { get; }

        /// <summary>
        /// The font style.
        /// </summary>
        public FontStyle FontStyle { get; }

        /// <summary>
        /// The line style to apply.
        /// </summary>
        public LineStyle LineStyle { get; }

        /// <summary>
        /// The visual style when the color is used in a marker.
        /// </summary>
        public MarkerVisualStyle MarkerVisualStyle { get; }

        internal AllColorableItemInfo ToAllColorableItemInfo(IVsFontAndColorUtilities utilities)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return new AllColorableItemInfo
            {
                bAutoBackgroundValid = 1,
                bAutoForegroundValid = 1,
                bDescriptionValid = 1,
                bFlagsValid = 1,
                bLineStyleValid = 1,
                bLocalizedNameValid = 1,
                bMarkerVisualStyleValid = 1,
                bNameValid = 1,
                bstrDescription = Description,
                bstrLocalizedName = LocalizedName,
                bstrName = Name,
                crAutoBackground = AutomaticBackground.ToColorRef(utilities),
                crAutoForeground = AutomaticForeground.ToColorRef(utilities),
                dwMarkerVisualStyle = (uint)MarkerVisualStyle,
                eLineStyle = (LINESTYLE)LineStyle,
                fFlags = (uint)Options,
                Info = new ColorableItemInfo
                {
                    bBackgroundValid = 1,
                    bFontFlagsValid = 1,
                    bForegroundValid = 1,
                    crBackground = DefaultBackground.ToColorRef(utilities),
                    crForeground = DefaultForeground.ToColorRef(utilities),
                    dwFontFlags = (uint)FontStyle
                }
            };
        }

        internal (uint Background, uint Foreground) GetColors(ref Guid category, ref ColorableItemInfo info, IVsFontAndColorUtilities utilities)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            AllColorableItemInfo[] allInfo = new[] { ToAllColorableItemInfo(utilities) };
            allInfo[0].Info = info;

            utilities.GetRGBOfItem(allInfo, ref category, out uint foreground, out uint background);

            return (background, foreground);
        }
    }
}
