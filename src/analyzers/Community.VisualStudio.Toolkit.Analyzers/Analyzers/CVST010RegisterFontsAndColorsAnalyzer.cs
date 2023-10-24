using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    /// <summary>
    /// Detects a package that declared fonts and colors, but does not register them during initialization.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CVST010RegisterFontsAndColorsAnalyzer : AnalyzerBase
    {
        internal const string DiagnosticId = Diagnostics.RegisterFontsAndColors;

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.CVST010_Title)),
            GetLocalizableString(nameof(Resources.CVST010_MessageFormat)),
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetLocalizableString(nameof(Resources.CVST010_Description)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol asyncPackageType;
            INamedTypeSymbol provideFontsAndColorsAttributeType;

            asyncPackageType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.AsyncPackage);
            provideFontsAndColorsAttributeType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.ProvideFontsAndColorsAttribute);

            if ((asyncPackageType is not null) && (provideFontsAndColorsAttributeType is not null))
            {
                context.RegisterSyntaxNodeAction(
                    (x) => AnalyzePackage(asyncPackageType, provideFontsAndColorsAttributeType, x),
                    SyntaxKind.ClassDeclaration
                );
            }
        }

        private static void AnalyzePackage(INamedTypeSymbol asyncPackageType, INamedTypeSymbol provideFontsAndColorsAttributeType, SyntaxNodeAnalysisContext context)
        {
            ClassDeclarationSyntax declaration = (ClassDeclarationSyntax)context.Node;
            INamedTypeSymbol classSymbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
            if (classSymbol is not null)
            {
                if (!classSymbol.IsAbstract && classSymbol.IsAssignableTo(asyncPackageType))
                {
                    if (ProvidesFontsAndColors(declaration, provideFontsAndColorsAttributeType, context))
                    {
                        MethodDeclarationSyntax? initializeAsync = declaration.FindAsyncPackageInitializeAsyncMethod();
                        if (initializeAsync is not null)
                        {
                            if (!RegistersFontAndColorProviders(initializeAsync))
                            {
                                // There is an `InitializeAsync` method, but it doesn't
                                // call the method to register fonts and colors.
                                context.ReportDiagnostic(Diagnostic.Create(_rule, initializeAsync.Identifier.GetLocation()));
                            }
                        }
                        else
                        {
                            // There is no `InitializeAsync` method, which means
                            // the fonts and colors cannot possible be registered.
                            context.ReportDiagnostic(Diagnostic.Create(_rule, declaration.Identifier.GetLocation()));
                        }
                    }
                }
            }
        }

        private static bool ProvidesFontsAndColors(ClassDeclarationSyntax declaration, INamedTypeSymbol provideFontsAndColorsAttributeType, SyntaxNodeAnalysisContext context)
        {
            foreach (AttributeListSyntax list in declaration.AttributeLists)
            {
                foreach (AttributeSyntax attribute in list.Attributes)
                {
                    ISymbol attributeSymbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
                    if (attributeSymbol is IMethodSymbol constructor)
                    {
                        if (constructor.ContainingType.IsAssignableTo(provideFontsAndColorsAttributeType))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool RegistersFontAndColorProviders(MethodDeclarationSyntax initializeAsync)
        {
            foreach (StatementSyntax statement in initializeAsync.Body.Statements)
            {
                if (statement is ExpressionStatementSyntax expression)
                {
                    if (expression.Expression is AwaitExpressionSyntax awaitExpression)
                    {
                        if (awaitExpression.Expression is InvocationExpressionSyntax invocation)
                        {
                            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                            {
                                if (memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                                {
                                    if (memberAccess.Name.Identifier.ValueText == "RegisterFontAndColorProvidersAsync")
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
