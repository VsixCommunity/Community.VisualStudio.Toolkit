using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CVST002DialogPageShouldBeComVisibleCodeFixProvider))]
    [Shared]
    public class CVST002DialogPageShouldBeComVisibleCodeFixProvider : CodeFixProviderBase
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(CVST002DialogPageShouldBeComVisibleAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (Diagnostic diagnostic in context.Diagnostics)
            {
                TextSpan span = diagnostic.Location.SourceSpan;
                ClassDeclarationSyntax? classDeclaration = root.FindToken(span.Start).Parent as ClassDeclarationSyntax;

                if (classDeclaration is not null)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            Resources.CVST002_CodeFix,
                            c => AddComVisibleAttributeAsync(context.Document, classDeclaration, c),
                            equivalenceKey: nameof(Resources.CVST002_CodeFix)
                        ),
                        diagnostic
                    );
                }
            }
        }

        private async Task<Document> AddComVisibleAttributeAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxEditor editor = new(root, document.Project.Solution.Workspace);
            SyntaxGenerator generator = editor.Generator;

            SyntaxNode attribute = generator.Attribute("ComVisible", new[] { generator.TrueLiteralExpression() });
            editor.AddAttribute(classDeclaration, attribute);

            root = editor.GetChangedRoot();

            // Add a using directive for the namespace
            // if the namespace is not already imported.
            if (root is CompilationUnitSyntax unit)
            {
                root = unit.WithUsings(AddUsingDirectiveIfMissing(unit.Usings, SyntaxFactory.ParseName("System.Runtime.InteropServices")));
            }

            return document.WithSyntaxRoot(root);
        }
    }
}
