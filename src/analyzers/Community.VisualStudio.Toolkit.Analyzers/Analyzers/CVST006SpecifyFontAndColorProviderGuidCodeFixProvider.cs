using System.Collections.Immutable;
using System.Composition;
using Community.VisualStudio.Toolkit.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CVST006SpecifyFontAndColorProviderGuidCodeFixProvider))]
    [Shared]
    public class CVST006SpecifyFontAndColorProviderGuidCodeFixProvider : MissingGuidAttributeCodeFixProviderBase
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(CVST006SpecifyFontAndColorProviderGuidAnalyzer.DiagnosticId);

        protected override string Title => Resources.CVST006_CodeFix;

        protected override string EquivalenceKey => nameof(Resources.CVST006_CodeFix);
    }
}
