using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Community.VisualStudio.Toolkit
{
    internal class DefaultColorizer : Colorizer
    {
        public DefaultColorizer(LanguageService svc, IVsTextLines buffer, IScanner? scanner) :
            base(svc, buffer, scanner)
        { }
    }
}
