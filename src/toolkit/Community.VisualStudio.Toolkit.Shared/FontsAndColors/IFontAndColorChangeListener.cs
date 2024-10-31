using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    internal interface IFontAndColorChangeListener
    {
        void SetFont(ref LOGFONTW logfont, ref FontInfo info);

        void SetColor(ColorDefinition definition, uint background, uint foreground, FontStyle fontStyle);
    }
}
