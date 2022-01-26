using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CVST004UseCorrectTypeInProvideProfileAttributeCodeFixProvider))]
    [Shared]
    public class CVST004UseCorrectTypeInProvideProfileAttributeCodeFixProvider : IncorrectProvidedTypeCodeFixProviderBase
    {
        protected override string FixableDiagnosticId => CVST004UseCorrectTypeInProvideProfileAttributeAnalyzer.DiagnosticId;

        protected override string ExpectedTypeName => KnownTypeNames.IProfileManager;
    }
}
