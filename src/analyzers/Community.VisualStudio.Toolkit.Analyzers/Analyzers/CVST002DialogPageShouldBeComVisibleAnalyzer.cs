using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CVST002DialogPageShouldBeComVisibleAnalyzer : AnalyzerBase
    {
        internal const string DiagnosticId = Diagnostics.DialogPageShouldBeComVisible;

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.CVST002_Title)),
            GetLocalizableString(nameof(Resources.CVST002_MessageFormat)),
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol? dialogPageType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.DialogPage);
            INamedTypeSymbol? comVisibleType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.ComVisibleAttribute);

            if (dialogPageType is not null && comVisibleType is not null)
            {
                context.RegisterSyntaxNodeAction((c) => AnalyzeClass(c, dialogPageType, comVisibleType), SyntaxKind.ClassDeclaration);
            }
        }

        private static void AnalyzeClass(SyntaxNodeAnalysisContext context, INamedTypeSymbol dialogPageType, INamedTypeSymbol comVisibleType)
        {
            ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
            ITypeSymbol? type = context.ContainingSymbol as ITypeSymbol;

            if (type is not null && type.IsSubclassOf(dialogPageType))
            {
                // This class inherits from `DialogPage`. It should contain
                // a `ComVisible` attribute with a parameter of `true`.
                foreach (AttributeData attribute in type.GetAttributes())
                {
                    if (attribute.AttributeClass.Equals(comVisibleType))
                    {
                        if (attribute.ConstructorArguments.Length == 1)
                        {
                            if (Equals(attribute.ConstructorArguments[0].Value, true))
                            {
                                return;
                            }
                        }
                    }
                }

                // The `ComVisible` attribute was not found, so report the diagnostic.
                context.ReportDiagnostic(Diagnostic.Create(_rule, classDeclaration.Identifier.GetLocation()));
            }
        }
    }
}
