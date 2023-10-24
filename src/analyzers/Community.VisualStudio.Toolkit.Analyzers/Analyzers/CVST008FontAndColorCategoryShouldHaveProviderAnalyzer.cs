using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    /// <summary>
    /// Detects a missing font and color provider.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CVST008FontAndColorCategoryShouldHaveProviderAnalyzer : AnalyzerBase
    {
        internal const string DiagnosticId = Diagnostics.DefineProviderForFontAndColorCategories;

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.CVST008_Title)),
            GetLocalizableString(nameof(Resources.CVST008_MessageFormat)),
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetLocalizableString(nameof(Resources.CVST008_Description)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            INamedTypeSymbol baseFontAndColorProviderType;
            INamedTypeSymbol baseFontAndColorCategoryType;

            baseFontAndColorProviderType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.BaseFontAndColorProvider);
            baseFontAndColorCategoryType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.BaseFontAndColorCategory);

            if ((baseFontAndColorProviderType is not null) && (baseFontAndColorCategoryType is not null))
            {
                State state = new(baseFontAndColorProviderType, baseFontAndColorCategoryType);

                context.RegisterSyntaxNodeAction(
                    (x) => RecordProvidersAndCategories(state, x),
                    SyntaxKind.ClassDeclaration
                );

                context.RegisterCompilationEndAction((endContext) =>
                {
                    if (state.Categories.Any() && !state.HasProviders)
                    {
                        // There are categories but no providers. Add a diagnostic to each category.
                        foreach (SyntaxToken token in state.Categories.Values)
                        {
                            endContext.ReportDiagnostic(Diagnostic.Create(_rule, token.GetLocation()));
                        }
                    }
                });
            }
        }

        private static void RecordProvidersAndCategories(State state, SyntaxNodeAnalysisContext context)
        {
            ClassDeclarationSyntax declaration = (ClassDeclarationSyntax)context.Node;
            INamedTypeSymbol classSymbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);

            if (classSymbol.IsSubclassOf(state.BaseFontAndColorProviderType))
            {
                state.HasProviders = true;
            }
            else if (classSymbol.IsSubclassOf(state.BaseFontAndColorCategoryType))
            {
                state.Categories[classSymbol] = declaration.Identifier;
            }
        }

        private class State
        {
            private int _hasProviders;

            public State(INamedTypeSymbol baseFontAndColorProviderType, INamedTypeSymbol baseFontAndColorCategoryType)
            {
                BaseFontAndColorProviderType = baseFontAndColorProviderType;
                BaseFontAndColorCategoryType = baseFontAndColorCategoryType;
            }

            public INamedTypeSymbol BaseFontAndColorProviderType { get; }

            public INamedTypeSymbol BaseFontAndColorCategoryType { get; }

            public ConcurrentDictionary<ISymbol, SyntaxToken> Categories { get; } = new();

            public bool HasProviders
            {
                get => Interlocked.CompareExchange(ref _hasProviders, 0, 0) != 0;
                set => Interlocked.Exchange(ref _hasProviders, value ? 1 : 0);
            }
        }
    }
}
