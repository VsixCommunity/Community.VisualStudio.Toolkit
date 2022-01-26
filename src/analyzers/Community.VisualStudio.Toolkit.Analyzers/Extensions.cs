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

        internal static bool IsSubclassOf(this ITypeSymbol? type, INamedTypeSymbol baseType)
        {
            while (type is not null)
            {
                if (type.Equals(baseType))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        internal static bool IsAssignableTo(this ITypeSymbol? type, INamedTypeSymbol targetType)
        {
            if (type is null)
            {
                return false;
            }

            switch (targetType.TypeKind)
            {
                case TypeKind.Class:
                    // If the target type is a class, then the type can only be assigned to
                    // it if it inherits from that type. There is no need to look at interfaces.
                    return type.IsSubclassOf(targetType);

                case TypeKind.Interface:
                    foreach (INamedTypeSymbol? interfaceType in type.AllInterfaces)
                    {
                        if (interfaceType.Equals(targetType))
                        {
                            return true;
                        }
                    }
                    return false;

                default:
                    return false;
            }
        }
    }
}
