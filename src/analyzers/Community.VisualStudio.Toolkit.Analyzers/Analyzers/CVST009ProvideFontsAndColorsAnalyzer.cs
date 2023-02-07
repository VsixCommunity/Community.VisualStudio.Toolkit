using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    /// <summary>
    /// Detects a package that should have a <c>ProvideFontsAndColorsAttribute</c> but doesn't.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CVST009ProvideFontsAndColorsAnalyzer : AnalyzerBase
    {
        internal const string DiagnosticId = Diagnostics.ProvideFontsAndColors;

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.CVST009_Title)),
            GetLocalizableString(nameof(Resources.CVST009_MessageFormat)),
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetLocalizableString(nameof(Resources.CVST009_Description)));

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
            INamedTypeSymbol baseFontAndColorProviderType;
            INamedTypeSymbol provideFontsAndColorsAttributeType;

            asyncPackageType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.AsyncPackage);
            baseFontAndColorProviderType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.BaseFontAndColorProvider);
            provideFontsAndColorsAttributeType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.ProvideFontsAndColorsAttribute);

            if (
                (asyncPackageType is not null) &&
                (baseFontAndColorProviderType is not null) &&
                (provideFontsAndColorsAttributeType is not null)
            )
            {
                State state = new(asyncPackageType, baseFontAndColorProviderType, provideFontsAndColorsAttributeType);

                context.RegisterSyntaxNodeAction(
                    (x) => RecordProvidersAndPackages(state, x),
                    SyntaxKind.ClassDeclaration
                );

                context.RegisterCompilationEndAction((endContext) =>
                {
                    CheckDeclarations(state, endContext);
                });
            }
        }

        private static void RecordProvidersAndPackages(State state, SyntaxNodeAnalysisContext context)
        {
            ClassDeclarationSyntax declaration = (ClassDeclarationSyntax)context.Node;
            INamedTypeSymbol classSymbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
            if (classSymbol is not null)
            {
                if (classSymbol.IsAssignableTo(state.BaseFontAndColorProviderType))
                {
                    state.Providers[classSymbol] = false;
                }

                if (classSymbol.IsAssignableTo(state.AsyncPackageType))
                {
                    state.Packages.Add(AnalyzePackage(declaration, context.SemanticModel, state));
                }
            }
        }

        private static Package AnalyzePackage(ClassDeclarationSyntax declaration, SemanticModel semanticModel, State state)
        {
            HashSet<ITypeSymbol> declaredProviders = new();
            foreach (AttributeListSyntax list in declaration.AttributeLists)
            {
                foreach (AttributeSyntax attribute in list.Attributes)
                {
                    if ((attribute.ArgumentList is not null) && (attribute.ArgumentList.Arguments.Count == 1))
                    {
                        ISymbol attributeSymbol = semanticModel.GetSymbolInfo(attribute).Symbol;
                        if (attributeSymbol is IMethodSymbol constructor)
                        {
                            if (constructor.ContainingType.IsAssignableTo(state.ProvideFontsAndColorsAttributeType))
                            {
                                if (attribute.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax typeofExpression)
                                {
                                    ITypeSymbol typeofType = semanticModel.GetTypeInfo(typeofExpression.Type).Type;
                                    if (typeofType is not null)
                                    {
                                        declaredProviders.Add(typeofType);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new Package(declaration, declaredProviders);
        }

        private static void CheckDeclarations(State state, CompilationAnalysisContext context)
        {
            if ((state.Providers.Count == 0) || (state.Packages.Count == 0))
            {
                return;
            }

            foreach (Package package in state.Packages)
            {
                foreach (ITypeSymbol provider in package.DeclaredProviders)
                {
                    state.Providers[provider] = true;
                }
            }

            if (state.Providers.Any((x) => !x.Value))
            {
                // Usually there's only one package, so just
                // report the diagnostics in the first one.
                ClassDeclarationSyntax packageToReport = state.Packages
                    .Select((x) => x.Declaration)
                    .OrderBy((x) => x.Identifier.ValueText)
                    .First();

                foreach (ISymbol provider in state.Providers.Where((x) => !x.Value).Select((x) => x.Key))
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            _rule,
                            packageToReport.Identifier.GetLocation(),
                            provider.Name
                        )
                    );
                }
            }
        }

        private class State : IDisposable
        {
            public State(
                INamedTypeSymbol asyncPackageType,
                INamedTypeSymbol baseFontAndColorProviderType,
                INamedTypeSymbol provideFontsAndColorsAttributeType
            )
            {
                AsyncPackageType = asyncPackageType;
                BaseFontAndColorProviderType = baseFontAndColorProviderType;
                ProvideFontsAndColorsAttributeType = provideFontsAndColorsAttributeType;
            }

            public INamedTypeSymbol AsyncPackageType { get; }

            public INamedTypeSymbol BaseFontAndColorProviderType { get; }

            public INamedTypeSymbol ProvideFontsAndColorsAttributeType { get; }

            public ConcurrentDictionary<ISymbol, bool> Providers { get; } = new();

            public ConcurrentBag<Package> Packages { get; } = new();

            public void Dispose()
            {
                // ConcurrentBag stores data per-thread, and that data remains in memory
                // until it is removed from the bag. Prevent a memory leak by emptying the bag.
                while (Packages.Count > 0)
                {
                    Packages.TryTake(out _);
                }
            }
        }

        private class Package
        {
            public Package(ClassDeclarationSyntax declaration, IReadOnlyCollection<ITypeSymbol> declaredProviders)
            {
                Declaration = declaration;
                DeclaredProviders = declaredProviders;
            }

            public ClassDeclarationSyntax Declaration { get; }

            public IReadOnlyCollection<ITypeSymbol> DeclaredProviders { get; }
        }
    }
}
