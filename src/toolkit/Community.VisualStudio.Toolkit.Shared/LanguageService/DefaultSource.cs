using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    internal class DefaultSource : Source
    {
        public DefaultSource(LanguageService service, IVsTextLines textLines, Colorizer colorizer)
            : base(service, textLines, colorizer)
        { }
    }
}

