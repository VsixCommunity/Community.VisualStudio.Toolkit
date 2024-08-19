using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    /// <summary>
    /// Detects font and color providers without an explicit GUID.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CVST006SpecifyFontAndColorProviderGuidAnalyzer : MissingGuidAttributeAnalyzerBase
    {
        internal const string DiagnosticId = Diagnostics.DefineExplicitGuidForFontAndColorProviders;

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.CVST006_Title)),
            GetLocalizableString(nameof(Resources.CVST006_MessageFormat)),
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetLocalizableString(nameof(Resources.CVST006_Description)));

        protected override DiagnosticDescriptor Descriptor => _rule;

        protected override string BaseTypeName => KnownTypeNames.BaseFontAndColorProvider;
    }
}
