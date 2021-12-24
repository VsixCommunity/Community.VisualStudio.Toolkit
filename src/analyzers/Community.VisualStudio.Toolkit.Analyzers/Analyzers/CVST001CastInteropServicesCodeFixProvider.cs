using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CVST001CastInteropServicesCodeFixProvider))]
    [Shared]
    public class CVST001CastInteropServicesCodeFixProvider : CodeFixProviderBase
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(CVST001CastInteropServicesAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                TextSpan span = diagnostic.Location.SourceSpan;
                SyntaxNode node = root.FindToken(span.Start).Parent;

                if (!diagnostic.Properties.TryGetValue(CVST001CastInteropServicesAnalyzer.RequiredTypeKey, out string requiredType))
                {
                    continue;
                }

                string title = string.Format(Resources.CVST001_CodeFix, requiredType);

                // The node at the start of the diagnostic's span could be one of two things.
                // Either it's a cast and we need to change the type used in the cast,
                // or it's an await expression and we need to add a cast statement.
                switch (node.Kind())
                {
                    case SyntaxKind.CastExpression:
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title,
                                c => FixIncorrectCastAsync(context.Document, (CastExpressionSyntax)node, requiredType, c),
                                equivalenceKey: nameof(Resources.CVST001_CodeFix)
                            ),
                            diagnostic);
                        break;

                    case SyntaxKind.AwaitExpression:
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                title,
                                c => FixMissingCastAsync(context.Document, (AwaitExpressionSyntax)node, requiredType, c),
                                equivalenceKey: nameof(Resources.CVST001_CodeFix)
                            ),
                            diagnostic);
                        break;

                }
            }
        }

        private static async Task<Document> FixIncorrectCastAsync(Document document, CastExpressionSyntax cast, string requiredType, CancellationToken cancellationToken)
        {
            // Split the type name into its namespace and type.
            QualifiedNameSyntax qualifiedName = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName(requiredType);
            NameSyntax namespaceName = qualifiedName.Left;
            TypeSyntax type = qualifiedName.Right;

            // Change the type in the cast statement.
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            CastExpressionSyntax? newCast = cast.WithType(type.WithTriviaFrom(cast.Type));

            // Update the cast statement and, if required, change the
            // type of the variable that the cast is being assigned to.
            root = await ReplaceNodeAndUpdateVariableTypeAsync(document, root, cast, newCast, type, cancellationToken);

            // Add a using directive for the namespace
            // if the namespace is not already imported.
            if (root is CompilationUnitSyntax unit)
            {
                root = unit.WithUsings(AddUsingDirectiveIfMissing(unit.Usings, namespaceName));
            }

            return document.WithSyntaxRoot(root);
        }

        private static async Task<Document> FixMissingCastAsync(Document document, ExpressionSyntax expression, string requiredType, CancellationToken cancellationToken)
        {
            // Split the type name into its namespace and type.
            QualifiedNameSyntax qualifiedName = (QualifiedNameSyntax)SyntaxFactory.ParseTypeName(requiredType);
            NameSyntax namespaceName = qualifiedName.Left;
            TypeSyntax type = qualifiedName.Right;

            // Add the cast statement.
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            CastExpressionSyntax? cast = SyntaxFactory.CastExpression(type, expression);

            // Replace the expression with the cast statement and, if required,
            // change the type of the variable that the expression was being assigned to.
            root = await ReplaceNodeAndUpdateVariableTypeAsync(document, root, expression, cast, type, cancellationToken);

            //// Add a using directive for the namespace
            //// if the namespace is not already imported.
            if (root is CompilationUnitSyntax unit)
            {
                root = unit.WithUsings(AddUsingDirectiveIfMissing(unit.Usings, namespaceName));
            }

            return document.WithSyntaxRoot(root);
        }

        private static async Task<SyntaxNode> ReplaceNodeAndUpdateVariableTypeAsync(Document document, SyntaxNode root, SyntaxNode originalNode, SyntaxNode newNode, TypeSyntax newType, CancellationToken cancellationToken)
        {
            // Check if the original node was the value being
            // assigned to the variable in a variable declaration.
            if (originalNode.Parent is EqualsValueClauseSyntax equalsSyntax)
            {
                if (IsDeclaratorForObject(equalsSyntax.Parent, out VariableDeclarationSyntax declaration))
                {
                    // The original node is being assigned to a variable
                    // of type `object`. Change the type of the variable
                    // and replace the original node with the new node.
                    return root.ReplaceNode(
                        declaration,
                        declaration.ReplaceNode(originalNode, newNode).WithType(newType.WithTriviaFrom(declaration.Type))
                    );
                }
            }
            else
            {
                // Check if the node is being assigned to something.
                if ((originalNode.Parent is AssignmentExpressionSyntax assignment) && (assignment.Left is IdentifierNameSyntax identifier))
                {
                    // If it's being assigned to a local variable or a field in the same
                    // document, then we will try to change the type of that variable or field.
                    SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);
                    SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(identifier);
                    if (symbolInfo.Symbol?.DeclaringSyntaxReferences.Length == 1)
                    {
                        VariableDeclarationSyntax? originalDeclaration = null;
                        VariableDeclarationSyntax? newDeclaration = null;

                        switch (symbolInfo.Symbol.Kind)
                        {
                            case SymbolKind.Local:
                                if (IsDeclaratorForObject(symbolInfo.Symbol.DeclaringSyntaxReferences[0].GetSyntax(), out originalDeclaration))
                                {
                                    newDeclaration = originalDeclaration.WithType(newType.WithTriviaFrom(originalDeclaration.Type));
                                }
                                break;

                            case SymbolKind.Field:
                                SyntaxNode fieldSyntax = symbolInfo.Symbol.DeclaringSyntaxReferences[0].GetSyntax();
                                // We can only change the type of the field if it's in the
                                // same document that we're replacing the original node in.
                                if (ReferenceEquals(fieldSyntax.SyntaxTree, originalNode.SyntaxTree))
                                {
                                    if (IsDeclaratorForObject(fieldSyntax, out originalDeclaration))
                                    {
                                        newDeclaration = originalDeclaration.WithType(newType.WithTriviaFrom(originalDeclaration.Type));
                                    }
                                }
                                break;
                        }

                        // If a declaration was found, then replace the original node
                        // with the new node, and replace the original declaration with
                        // the new declaration that will change the type of the variable.
                        if (originalDeclaration is not null && newDeclaration is not null)
                        {
                            return root.ReplaceNodes(
                                new[] { originalNode, originalDeclaration },
                                (node, _) => (node == originalNode) ? newNode : newDeclaration
                            );
                        }
                    }
                }
            }

            // If we get to this point, then the original node is not part
            // of a statement that requires us to update any variable types,
            // so just replace the original node with the new node.
            return root.ReplaceNode(originalNode, newNode);
        }

        private static bool IsDeclaratorForObject(SyntaxNode node, out VariableDeclarationSyntax variableDeclaration)
        {
            if (node is VariableDeclaratorSyntax declarator)
            {
                if (declarator.Parent is VariableDeclarationSyntax declarationSyntax)
                {
                    if (declarationSyntax.Type is PredefinedTypeSyntax predefined)
                    {
                        if (predefined.Keyword.IsKind(SyntaxKind.ObjectKeyword))
                        {
                            variableDeclaration = declarationSyntax;
                            return true;
                        }
                    }
                }
            }

            variableDeclaration = null!;
            return false;
        }
    }
}
