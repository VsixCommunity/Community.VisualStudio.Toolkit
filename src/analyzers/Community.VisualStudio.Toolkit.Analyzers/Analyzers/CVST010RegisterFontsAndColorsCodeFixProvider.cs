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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CVST010RegisterFontsAndColorsCodeFixProvider))]
    [Shared]
    public class CVST010RegisterFontsAndColorsCodeFixProvider : CodeFixProviderBase
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(CVST010RegisterFontsAndColorsAnalyzer.DiagnosticId);

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
                        Resources.CVST010_CodeFix,
                        cancellation => AddInitializeAsyncMethodAsync(context.Document, declaration, cancellation),
                        equivalenceKey: nameof(Resources.CVST010_CodeFix)
                    ),
                    diagnostic
                );
            }
            else if (node is MethodDeclarationSyntax method)
            {
                // There is an `InitializeAsync()` method. We can fix the
                // diagnostics by adding a statement to that method. 
                context.RegisterCodeFix(
                    CodeAction.Create(
                        Resources.CVST010_CodeFix,
                        cancellation => AddRegisterFontAndColorProvidersCallAsync(context.Document, method, cancellation),
                        equivalenceKey: nameof(Resources.CVST010_CodeFix)
                    ),
                    diagnostic
                );
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
                        generator.ParameterDeclaration("cancellationToken", SyntaxFactory.ParseTypeName("System.Threading.CancellationToken")),
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
                                    "RegisterFontAndColorProvidersAsync"
                                )
                            )
                        )
                    }
                ).WithAdditionalAnnotations(Simplifier.Annotation)
            );

            return document.WithSyntaxRoot(editor.GetChangedRoot());
        }

        private async Task<Document> AddRegisterFontAndColorProvidersCallAsync(Document document, MethodDeclarationSyntax initializeAsyncMethod, CancellationToken cancellationToken)
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
                                    "RegisterFontAndColorProvidersAsync"
                                )
                            )
                        )
                    ).WithAdditionalAnnotations(Simplifier.Annotation)
                )
            );

            return document.WithSyntaxRoot(editor.GetChangedRoot());
        }
    }
}
