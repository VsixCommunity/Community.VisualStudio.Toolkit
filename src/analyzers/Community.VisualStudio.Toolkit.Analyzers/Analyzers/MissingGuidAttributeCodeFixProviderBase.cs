using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Community.VisualStudio.Toolkit.Analyzers.Analyzers
{
    public abstract class MissingGuidAttributeCodeFixProviderBase : CodeFixProviderBase
    {
        protected abstract string Title { get; }

        protected abstract string EquivalenceKey { get; }

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan span = diagnostic.Location.SourceSpan;
            SyntaxNode node = root.FindToken(span.Start).Parent;

            if (node is ClassDeclarationSyntax declaration)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        Title,
                        cancellation => AddGuidAttributeAsync(context.Document, declaration, cancellation),
                        equivalenceKey: EquivalenceKey
                    ),
                    diagnostic
                );
            }
        }

        private async Task<Document> AddGuidAttributeAsync(Document document, ClassDeclarationSyntax declaration, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxEditor editor = new(root, document.Project.Solution.Workspace);
            SyntaxGenerator generator = editor.Generator;

            editor.AddAttribute(
                declaration,
                generator.Attribute(
                    "System.Runtime.InteropServices.GuidAttribute",
                    generator.LiteralExpression(Guid.NewGuid().ToString())
                ).WithAdditionalAnnotations(Simplifier.Annotation)
            );

            return document.WithSyntaxRoot(editor.GetChangedRoot());
        }
    }
}
