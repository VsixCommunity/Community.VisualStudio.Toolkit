using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    public abstract class IncorrectProvidedTypeCodeFixProviderBase : CodeFixProviderBase
    {
        private ImmutableArray<string>? _fixableDiagnosticIds;

        protected abstract string ExpectedTypeName { get; }

        protected abstract string FixableDiagnosticId { get; }

        public sealed override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds ??= ImmutableArray.Create(FixableDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                // The convention used by the toolkit for some types is to define a container
                // class for implementations of the provided types. For example, you might
                // define a container class for all of the DialogPage implementations.
                //
                // So, find all of the nested types that inherit from
                // the expected type and use their name as a suggested fix.
                TypeSyntax? actualTypeSyntax = root.FindNode(diagnostic.Location.SourceSpan) as TypeSyntax;
                if (actualTypeSyntax is not null)
                {
                    SemanticModel semanticModel = await context.Document.GetSemanticModelAsync();
                    INamedTypeSymbol? expectedType = semanticModel.Compilation.GetTypeByMetadataName(ExpectedTypeName);
                    if (expectedType is not null)
                    {
                        if (semanticModel.GetSymbolInfo(actualTypeSyntax).Symbol is ITypeSymbol argumentType)
                        {
                            IEnumerable<INamedTypeSymbol> nestedTypes = argumentType
                                .GetTypeMembers()
                                .Where((x) => x.IsAssignableTo(expectedType))
                                .OrderBy((x) => x.Name);

                            foreach (INamedTypeSymbol nestedType in nestedTypes)
                            {
                                string title = string.Format(Resources.IncorrectProvidedType_CodeFix, $"{argumentType.Name}.{nestedType.Name}");
                                context.RegisterCodeFix(
                                    CodeAction.Create(
                                        title,
                                        c => ChangeTypeNameAsync(context.Document, actualTypeSyntax, nestedType, c),
                                        equivalenceKey: $"{FixableDiagnosticId}:{title}"
                                    ),
                                    diagnostic
                                );
                            }
                        }
                    }
                }
            }
        }

        private static async Task<Document> ChangeTypeNameAsync(Document document, SyntaxNode nodeToChange, INamedTypeSymbol newType, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxEditor editor = new(root, document.Project.Solution.Workspace);
            SyntaxGenerator generator = editor.Generator;

            editor.ReplaceNode(nodeToChange, generator.NameExpression(newType));

            return document.WithSyntaxRoot(editor.GetChangedRoot());
        }

    }
}
