using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    public abstract class IncorrectProvidedTypeAnalyzerBase : AnalyzerBase
    {
        protected abstract string AttributeTypeName { get; }

        protected abstract string ExpectedTypeName { get; }

        protected abstract DiagnosticDescriptor Descriptor { get; }

        public sealed override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol? attributeType = context.Compilation.GetTypeByMetadataName(AttributeTypeName);
            INamedTypeSymbol? expectedType = context.Compilation.GetTypeByMetadataName(ExpectedTypeName);

            if (attributeType is not null && expectedType is not null)
            {
                context.RegisterSyntaxNodeAction(
                    (c) => AnalyzeAttribute(c, attributeType, expectedType),
                    SyntaxKind.Attribute
                );
            }
        }

        private void AnalyzeAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol attributeType, INamedTypeSymbol expectedType)
        {
            AttributeSyntax attribute = (AttributeSyntax)context.Node;
            if (context.SemanticModel.GetTypeInfo(attribute).Type?.IsAssignableTo(attributeType) == true)
            {
                // The type that is provided is always the first argument to the attribute's constructor.
                AttributeArgumentSyntax? typeArgument = attribute.ArgumentList?.Arguments.FirstOrDefault();
                if (typeArgument?.Expression is TypeOfExpressionSyntax typeOfExpression)
                {
                    ISymbol? argumentSymbol = context.SemanticModel.GetSymbolInfo(typeOfExpression.Type).Symbol;
                    if (argumentSymbol is ITypeSymbol argumentTypeSymbol)
                    {
                        if (!argumentTypeSymbol.IsAssignableTo(expectedType))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptor,
                                    typeOfExpression.Type.GetLocation(),
                                    typeOfExpression.Type.GetText(),
                                    ExpectedTypeName
                                )
                            );
                        }
                    }
                }
            }
        }
    }
}
