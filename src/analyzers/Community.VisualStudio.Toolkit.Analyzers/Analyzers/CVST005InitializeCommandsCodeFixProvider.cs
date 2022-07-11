using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CVST005InitializeCommandsCodeFixProvider))]
    [Shared]
    public class CVST005InitializeCommandsCodeFixProvider : CodeFixProviderBase
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(CVST005InitializeCommandsAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan span = diagnostic.Location.SourceSpan;
            SyntaxNode node = root.FindToken(span.Start).Parent;

            if (node is ClassDeclarationSyntax declaration)
            {
                // There is no `InitializeAsync()` method, so the diagnostic
                // was reported on the class declaration. To fix all of the 
                // diagnostics, we can create the `InitializeAsync()` method.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        Resources.CVST005_CodeFix,
                        cancellation => AddInitializeAsyncMethodAsync(context.Document, declaration, cancellation),
                        equivalenceKey: nameof(Resources.CVST001_CodeFix)
                    ),
                    diagnostic
                );
            }
            else if (node is MethodDeclarationSyntax method)
            {
                // There is an `InitializeAsync()` method. We can fix the
                // diagnostics by adding statements to that method. If each
                // diagnostic contains a property for the command name, then
                // we need to register the commands individually. If no name
                // is specified, then we can initialize all commands in bulk.
                if (context.Diagnostics.All((x) => x.Properties.ContainsKey(CVST005InitializeCommandsAnalyzer.CommandNameKey)))
                {
                    List<string> commandNames = context.Diagnostics
                        .Select((x) => x.Properties[CVST005InitializeCommandsAnalyzer.CommandNameKey])
                        .ToList();
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            Resources.CVST005_CodeFix,
                            cancellation => InitializeSpecificCommandsAsync(context.Document, method, commandNames, cancellation),
                            equivalenceKey: nameof(Resources.CVST001_CodeFix)
                        ),
                        diagnostic
                    );
                }
                else
                {
                    // The command names weren't specified in the diagnostic, which means
                    // that no existing commands are being initialized. We can choose to 
                    // initialize the command individually or initialize all commands
                    // in bulk. We'll choose the bulk method because it's shorter.
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            Resources.CVST005_CodeFix,
                            cancellation => InitializeAllCommandsAsync(context.Document, method, cancellation),
                            equivalenceKey: nameof(Resources.CVST001_CodeFix)
                        ),
                        diagnostic
                    );
                }
            }
        }

        private async Task<Document> AddInitializeAsyncMethodAsync(Document document, ClassDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxEditor editor = new(root, document.Project.Solution.Workspace);
            SyntaxGenerator generator = editor.Generator;

            editor.AddMember(
                declaration,
                generator.MethodDeclaration(
                    "InitializeAsync",
                    parameters: new[] {
                        generator.ParameterDeclaration("cancellationToken",SyntaxFactory.ParseTypeName("System.Threading.CancellationToken")),
                        generator.ParameterDeclaration("progress", SyntaxFactory.ParseTypeName("System.IProgress<Microsoft.VisualStudio.Shell.ServiceProgressData>"))
                    },
                    returnType: SyntaxFactory.ParseTypeName("System.Threading.Tasks.Task"),
                    accessibility: Accessibility.Protected,
                    modifiers: DeclarationModifiers.Override | DeclarationModifiers.Async,
                    statements: new[] {
                        generator.AwaitExpression(
                            generator.InvocationExpression(
                                generator.MemberAccessExpression(
                                    generator.ThisExpression(),
                                    "RegisterCommandsAsync"
                                )
                            )
                        )
                    }
                ).WithAdditionalAnnotations(Simplifier.Annotation)
            );

            return document.WithSyntaxRoot(editor.GetChangedRoot());
        }

        private async Task<Document> InitializeAllCommandsAsync(Document document, MethodDeclarationSyntax initializeAsyncMethod, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxEditor editor = new(root, document.Project.Solution.Workspace);
            SyntaxGenerator generator = editor.Generator;

            editor.SetStatements(
                initializeAsyncMethod,
                initializeAsyncMethod.Body.Statements.Add(
                    (StatementSyntax)generator.ExpressionStatement(
                        generator.AwaitExpression(
                            generator.InvocationExpression(
                                generator.MemberAccessExpression(
                                    generator.ThisExpression(),
                                    "RegisterCommandsAsync"
                                )
                            )
                        )
                    ).WithAdditionalAnnotations(Simplifier.Annotation)
                )
            );

            return document.WithSyntaxRoot(editor.GetChangedRoot());
        }

        private async Task<Document> InitializeSpecificCommandsAsync(Document document, MethodDeclarationSyntax initializeAsyncMethod, IEnumerable<string> commandNames, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxEditor editor = new(root, document.Project.Solution.Workspace);
            SyntaxGenerator generator = editor.Generator;

            editor.SetStatements(
                initializeAsyncMethod,
                initializeAsyncMethod.Body.Statements.AddRange(
                    commandNames.Select((name) => (StatementSyntax)generator.ExpressionStatement(
                        generator.AwaitExpression(
                            generator.InvocationExpression(
                                generator.MemberAccessExpression(
                                    SyntaxFactory.ParseTypeName(name),
                                    "InitializeAsync"
                                ),
                                generator.ThisExpression()
                            )
                        )
                    ).WithAdditionalAnnotations(Simplifier.Annotation)
                ))
            );

            return document.WithSyntaxRoot(editor.GetChangedRoot());
        }

    }
}
