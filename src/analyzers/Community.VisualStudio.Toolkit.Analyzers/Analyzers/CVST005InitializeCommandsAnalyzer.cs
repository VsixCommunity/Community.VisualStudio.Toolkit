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
    /// Detects commands that have not been initialized.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CVST005InitializeCommandsAnalyzer : AnalyzerBase
    {
        internal const string DiagnosticId = Diagnostics.InitializeCommands;
        internal const string CommandNameKey = "CommandName";

        private static readonly DiagnosticDescriptor _rule = new(
            DiagnosticId,
            GetLocalizableString(nameof(Resources.CVST005_Title)),
            GetLocalizableString(nameof(Resources.CVST005_MessageFormat)),
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: GetLocalizableString(nameof(Resources.CVST005_Description)));

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
            INamedTypeSymbol baseCommandType;


            asyncPackageType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.AsyncPackage);
            baseCommandType = context.Compilation.GetTypeByMetadataName(KnownTypeNames.BaseCommand);

            if ((asyncPackageType is not null) && (baseCommandType is not null))
            {
                State state = new(asyncPackageType, baseCommandType);

                context.RegisterSyntaxNodeAction(
                    (x) => RecordCommandsAndPackages(state, x),
                    SyntaxKind.ClassDeclaration
                );

                context.RegisterCompilationEndAction((x) =>
                {
                    CheckInitialization(state, x);
                    state.Dispose();
                });
            }
        }

        private static void RecordCommandsAndPackages(State state, SyntaxNodeAnalysisContext context)
        {
            ClassDeclarationSyntax declaration = (ClassDeclarationSyntax)context.Node;
            INamedTypeSymbol classSymbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
            if (classSymbol is not null)
            {
                // Abstract classes cannot be created, which means that even if this class
                // inherits from `BaseCommand`, it's not a command that can be registered.
                if (!declaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
                {
                    if (IsCommand(classSymbol, state.BaseCommandType))
                    {
                        state.Commands[classSymbol] = false;
                    }
                }

                if (classSymbol.IsAssignableTo(state.AsyncPackageType))
                {
                    state.Packages.Add((declaration, context.SemanticModel));
                }
            }
        }

        private static bool IsCommand(INamedTypeSymbol classType, INamedTypeSymbol baseCommandType)
        {
            INamedTypeSymbol? baseType = classType.BaseType;
            while (baseType is not null)
            {
                if (baseType.IsGenericType && baseType.OriginalDefinition.Equals(baseCommandType))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        private static void CheckInitialization(State state, CompilationAnalysisContext context)
        {
            if ((state.Commands.Count == 0) || (state.Packages.Count == 0))
            {
                return;
            }

            bool initializeIndividually = false;
            foreach ((ClassDeclarationSyntax Class, SemanticModel SemanticModel) package in state.Packages)
            {
                switch (CheckInitializationInPackage(package.Class, package.SemanticModel, state.Commands))
                {
                    case InitializationMode.Bulk:
                        // All commands are being initialized in bulk, 
                        // so we don't need to check anything else.
                        return;

                    case InitializationMode.Individual:
                        // Commands are being initialized individually. If we have
                        // to report diagnostics, we'll tell the code fix that the
                        // uninitialized commands should be initialized individually.
                        initializeIndividually = true;
                        break;

                }
            }

            if (state.Commands.Any((x) => !x.Value))
            {
                // Usually there's only one package, so just
                // report the diagnostics in the first one.
                ClassDeclarationSyntax packageToReport = state.Packages
                    .Select((x) => x.Class)
                    .OrderBy((x) => x.Identifier.ValueText)
                    .First();

                MethodDeclarationSyntax? initializeAsyncMethod = FindInitializeAsyncMethod(packageToReport);

                foreach (INamedTypeSymbol command in state.Commands.Where((x) => !x.Value).Select((x) => x.Key))
                {

                    // If the commands should be initialized individually,
                    // then we need to include a property in the diagnostic
                    // that tells the code fix what the name of the command is.
                    ImmutableDictionary<string, string> properties =
                        initializeIndividually ?
                        ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, string>(CommandNameKey, command.Name) }) :
                        ImmutableDictionary<string, string>.Empty;

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            _rule,
                            (initializeAsyncMethod?.Identifier ?? packageToReport.Identifier).GetLocation(),
                            properties,
                            command.Name
                        )
                    );
                }
            }
        }

        private static InitializationMode? CheckInitializationInPackage(
            ClassDeclarationSyntax package,
            SemanticModel semanticModel,
            ConcurrentDictionary<ISymbol, bool> commands
        )
        {
            InitializationMode? mode = null;

            MethodDeclarationSyntax? initializeAsync = FindInitializeAsyncMethod(package);
            if (initializeAsync is not null)
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
                                        if (memberAccess.Name.Identifier.ValueText == "RegisterCommandsAsync")
                                        {
                                            // The statement is `this.RegisterCommandsAsync()`.
                                            return InitializationMode.Bulk;
                                        }
                                    }
                                    else if (memberAccess.Name.Identifier.ValueText == "InitializeAsync")
                                    {
                                        SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(memberAccess.Expression);
                                        if (commands.ContainsKey(symbolInfo.Symbol))
                                        {
                                            // The statement is `Command.InitializeAsync(package)`.
                                            commands[symbolInfo.Symbol] = true;
                                            mode = InitializationMode.Individual;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return mode;
        }

        internal static MethodDeclarationSyntax? FindInitializeAsyncMethod(ClassDeclarationSyntax package)
        {
            foreach (MethodDeclarationSyntax method in package.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.Modifiers.Any(SyntaxKind.ProtectedKeyword) && method.Modifiers.Any(SyntaxKind.OverrideKeyword))
                {
                    if (string.Equals(method.Identifier.ValueText, "InitializeAsync"))
                    {
                        return method;
                    }
                }
            }

            return null;
        }

        private enum InitializationMode
        {
            Individual,
            Bulk
        }

        private class State : IDisposable
        {
            public State(INamedTypeSymbol asyncPackageType, INamedTypeSymbol baseCommandType)
            {
                AsyncPackageType = asyncPackageType;
                BaseCommandType = baseCommandType;
            }

            public INamedTypeSymbol AsyncPackageType { get; }

            public INamedTypeSymbol BaseCommandType { get; }

            public ConcurrentDictionary<ISymbol, bool> Commands { get; } = new();

            public ConcurrentBag<(ClassDeclarationSyntax Class, SemanticModel SemanticModel)> Packages { get; } = new();

            public void Dispose()
            {
                // ConcurrentBag stores data per-thread, and thata data remains in memory
                // until it is removed from the bag. Prevent a memory leak by emptying the bag.
                while (Packages.Count > 0)
                {
                    Packages.TryTake(out _);
                }
            }
        }
    }
}
