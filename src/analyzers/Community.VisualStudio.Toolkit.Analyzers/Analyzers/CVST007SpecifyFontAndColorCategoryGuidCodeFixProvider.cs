using System.Collections.Immutable;
using System.Composition;
using Community.VisualStudio.Toolkit.Analyzers.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CVST007SpecifyFontAndColorCategoryGuidCodeFixProvider))]
    [Shared]
    public class CVST007SpecifyFontAndColorCategoryGuidCodeFixProvider : MissingGuidAttributeCodeFixProviderBase
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(CVST007SpecifyFontAndColorCategoryGuidAnalyzer.DiagnosticId);

        protected override string Title => Resources.CVST007_CodeFix;

        protected override string EquivalenceKey => nameof(Resources.CVST007_CodeFix);
    }
}
