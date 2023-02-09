using System.Runtime.InteropServices;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace TestExtension
{
    [Guid("bfbcd352-3b43-4034-b951-7ca841a16c81")]
    public class DemoFontAndColorCategory : BaseFontAndColorCategory<DemoFontAndColorCategory>
    {
        public DemoFontAndColorCategory() : base(new FontDefinition("Consolas", 12)) { }

        public override string Name => "Fonts and Colors Demo";

        public ColorDefinition TopLeft { get; } = new(
            "Top Left",
            defaultBackground: VisualStudioColor.Indexed(COLORINDEX.CI_RED),
            defaultForeground: VisualStudioColor.Indexed(COLORINDEX.CI_WHITE),
            options: ColorDefinition.DefaultOptions | ColorOptions.AllowBoldChange
        );

        public ColorDefinition TopRight { get; } = new(
            "Top Right",
            defaultBackground: VisualStudioColor.Automatic(),
            defaultForeground: VisualStudioColor.Automatic(),
            automaticBackground: VisualStudioColor.VsColor(__VSSYSCOLOREX.VSCOLOR_ENVIRONMENT_BACKGROUND),
            automaticForeground: VisualStudioColor.VsColor(__VSSYSCOLOREX.VSCOLOR_PANEL_TEXT),
            options: ColorDefinition.DefaultOptions | ColorOptions.AllowBoldChange
        );

        public ColorDefinition BottomLeft { get; } = new(
            "Bottom Left",
            defaultBackground: VisualStudioColor.SysColor(13),
            defaultForeground: VisualStudioColor.SysColor(14),
            options: ColorOptions.AllowBackgroundChange | ColorOptions.AllowForegroundChange
        );

        public ColorDefinition BottomRight { get; } = new(
            "Bottom Right",
            defaultBackground: VisualStudioColor.Indexed(COLORINDEX.CI_DARKGREEN),
            defaultForeground: VisualStudioColor.Indexed(COLORINDEX.CI_WHITE)
        );
    }
}
