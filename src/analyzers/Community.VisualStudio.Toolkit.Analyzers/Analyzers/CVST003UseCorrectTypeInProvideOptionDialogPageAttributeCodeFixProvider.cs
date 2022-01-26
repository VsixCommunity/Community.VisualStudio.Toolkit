using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CVST003UseCorrectTypeInProvideOptionDialogPageAttributeCodeFixProvider))]
    [Shared]
    public class CVST003UseCorrectTypeInProvideOptionDialogPageAttributeCodeFixProvider : IncorrectProvidedTypeCodeFixProviderBase
    {
        protected override string FixableDiagnosticId => CVST003UseCorrectTypeInProvideOptionDialogPageAttributeAnalyzer.DiagnosticId;

        protected override string ExpectedTypeName => KnownTypeNames.DialogPage;
    }
}
