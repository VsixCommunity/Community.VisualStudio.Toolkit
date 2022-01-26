using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CVST003UseCorrectTypeInProvideOptionDialogPageAttributeAnalyzer : IncorrectProvidedTypeAnalyzerBase
    {
        internal const string DiagnosticId = Diagnostics.UseCorrectTypeInProvideOptionDialogPageAttribute;

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.CVST003_Title)),
            GetLocalizableString(nameof(Resources.IncorrectProvidedType_MessageFormat)),
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(_rule);

        protected override string AttributeTypeName => KnownTypeNames.ProvideOptionDialogPageAttribute;

        protected override string ExpectedTypeName => KnownTypeNames.DialogPage;

        protected override DiagnosticDescriptor Descriptor => _rule;
    }
}
