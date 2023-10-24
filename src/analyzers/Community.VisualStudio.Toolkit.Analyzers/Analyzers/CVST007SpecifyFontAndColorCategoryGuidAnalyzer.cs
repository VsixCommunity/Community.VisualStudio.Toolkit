using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    /// <summary>
    /// Detects font and color categories without an explicit GUID.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CVST007SpecifyFontAndColorCategoryGuidAnalyzer : MissingGuidAttributeAnalyzerBase
    {
        internal const string DiagnosticId = Diagnostics.DefineExplicitGuidForFontAndColorCategories;

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.CVST007_Title)),
            GetLocalizableString(nameof(Resources.CVST007_MessageFormat)),
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetLocalizableString(nameof(Resources.CVST007_Description)));

        protected override DiagnosticDescriptor Descriptor => _rule;

        protected override string BaseTypeName => KnownTypeNames.BaseFontAndColorCategory;
    }
}
