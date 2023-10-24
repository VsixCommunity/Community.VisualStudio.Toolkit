using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    public abstract class MissingGuidAttributeAnalyzerBase : AnalyzerBase
    {
        private ImmutableArray<DiagnosticDescriptor>? _supportedDiagnostics;

        protected abstract DiagnosticDescriptor Descriptor { get; }

        protected abstract string BaseTypeName { get; }

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            _supportedDiagnostics ??= ImmutableArray.Create(Descriptor);

        public sealed override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol baseType;
            INamedTypeSymbol guidAttributeType;

            baseType = context.Compilation.GetTypeByMetadataName(BaseTypeName);
            guidAttributeType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.GuidAttribute);

            if ((baseType is not null) && (guidAttributeType is not null))
            {
                context.RegisterSyntaxNodeAction(
                    (x) => AnalyzeClassDeclaration(baseType, guidAttributeType, x),
                    SyntaxKind.ClassDeclaration
                );
            }
        }

        private void AnalyzeClassDeclaration(
            INamedTypeSymbol baseType,
            INamedTypeSymbol guidAttributeType,
            SyntaxNodeAnalysisContext context
        )
        {
            ClassDeclarationSyntax declaration = (ClassDeclarationSyntax)context.Node;
            INamedTypeSymbol classType = context.SemanticModel.GetDeclaredSymbol(declaration);

            if (classType.IsSubclassOf(baseType))
            {
                foreach (AttributeListSyntax list in declaration.AttributeLists)
                {
                    foreach (AttributeSyntax attribute in list.Attributes)
                    {
                        ISymbol attributeSymbol = context.SemanticModel.GetTypeInfo(attribute).Type;
                        if (guidAttributeType.Equals(attributeSymbol))
                        {
                            return;
                        }
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, declaration.Identifier.GetLocation()));
            }
        }
    }
}
