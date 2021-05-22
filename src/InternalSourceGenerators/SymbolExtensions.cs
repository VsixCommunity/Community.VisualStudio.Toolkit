using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

namespace InternalSourceGenerators
{
    internal static class SymbolExtensions
    {
        public static string GetLeadingTriviaFrom<T>(this ISymbol symbol, CancellationToken cancellationToken) where T : SyntaxNode
        {
            T? syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken).FirstAncestorOrSelf<T>();
            return syntax?.GetLeadingTrivia().ToFullString() ?? "";
        }
    }
}
