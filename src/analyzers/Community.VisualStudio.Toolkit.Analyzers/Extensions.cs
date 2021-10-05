using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    internal static class Extensions
    {
        internal static bool IsVsProperty(this ExpressionSyntax expression, string propertyName)
        {
            if (expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                MemberAccessExpressionSyntax access = (MemberAccessExpressionSyntax)expression;
                return string.Equals(access.Name.Identifier.Text, propertyName) && IsVS(access.Expression);
            }

            return false;
        }

        internal static bool IsVS(this ExpressionSyntax expression)
        {
            return expression.IsKind(SyntaxKind.IdentifierName) &&
                string.Equals(((IdentifierNameSyntax)expression).Identifier.Text, "VS");
        }
    }
}
