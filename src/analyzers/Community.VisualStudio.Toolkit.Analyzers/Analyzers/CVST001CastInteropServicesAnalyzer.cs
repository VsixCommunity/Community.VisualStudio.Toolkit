using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    /// <summary>
    /// Detects calls to <c>VS.Services.*</c> properties that return 
    /// a <c>Task&lt;object&gt;</c> and are not being cast to the correct type.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CVST001CastInteropServicesAnalyzer : AnalyzerBase
    {
        internal const string DiagnosticId = Diagnostics.CastInteropTypes;
        internal const string RequiredTypeKey = "RequiredType";

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.CVST001_Title)),
            GetLocalizableString(nameof(Resources.CVST001_MessageFormat)),
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetLocalizableString(nameof(Resources.CVST001_Description)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(_rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
        {
            MemberAccessExpressionSyntax memberAccess = (MemberAccessExpressionSyntax)context.Node;
            if (memberAccess.Expression.IsVsProperty("Services"))
            {
                // We'll only check calls that are within an `await` expression, because all 
                // services are retrieved asynchronously. Do this before we get the symbol of
                // the method that's being called, because getting the parent will be quicker.
                SyntaxNode? awaitExpression = GetParentAwaitExpression(memberAccess);
                if (awaitExpression is null)
                {
                    return;
                }

                // Get the method's symbol to see if it declares a specific type to cast to.
                SymbolInfo methodSymbol = context.SemanticModel.GetSymbolInfo(memberAccess.Name);

                if (methodSymbol.Symbol is not null)
                {
                    // If the service needs to be cast to a specific type, it will
                    // be indicated by a `CastTo` attribute, where that attribute's
                    // constructor parameter is the full name of the type to cast to.
                    string? serviceType = methodSymbol
                        .Symbol
                        .GetAttributes()
                        .Where(x => string.Equals(x.AttributeClass.Name, "CastToAttribute"))
                        .Select(x => x.ConstructorArguments.FirstOrDefault().Value as string)
                        .FirstOrDefault();

                    if (serviceType is not null)
                    {
                        Location? location = null;

                        if (awaitExpression.Parent.IsKind(SyntaxKind.CastExpression))
                        {
                            // The service is already being cast. Check that it's being cast to the correct type.
                            CastExpressionSyntax cast = (CastExpressionSyntax)awaitExpression.Parent;
                            SymbolInfo castSymbol = context.SemanticModel.GetSymbolInfo(cast.Type);
                            string? fullName = castSymbol.Symbol?.ToString();

                            if (!string.Equals(fullName, serviceType))
                            {
                                location = cast.GetLocation();
                            }
                        }
                        else
                        {
                            // The service is not being cast at all.
                            location = awaitExpression.GetLocation();
                        }

                        if (location is not null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(_rule, location, CreateProperties(RequiredTypeKey, serviceType), serviceType));
                        }
                    }
                }
            }
        }

        private static SyntaxNode? GetParentAwaitExpression(SyntaxNode node)
        {
            while (node is not null)
            {
                switch (node.Parent.Kind())
                {
                    case SyntaxKind.AwaitExpression:
                        return node.Parent;

                    case SyntaxKind.SimpleMemberAccessExpression:
                        // The original node could be part of a method call chain,
                        // so this is fine. Step up to the parent and try again.
                        node = node.Parent;
                        break;

                    case SyntaxKind.InvocationExpression:
                        // We can start from a member access expression, so it's
                        // perfectly reasonable to invoke that member before awaiting
                        // the result. Step up to the parent and try again.
                        node = node.Parent;
                        break;

                    default:
                        return null;
                }
            }

            return null;
        }
    }
}
