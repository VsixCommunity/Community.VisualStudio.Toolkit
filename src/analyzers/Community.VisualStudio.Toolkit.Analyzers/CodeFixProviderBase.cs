using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    public abstract class CodeFixProviderBase : CodeFixProvider
    {
        protected static SyntaxList<UsingDirectiveSyntax> AddUsingDirectiveIfMissing(SyntaxList<UsingDirectiveSyntax> usings, NameSyntax namespaceName)
        {
            string namespaceToImport = namespaceName.ToString();
            int? insertionIndex = null;

            // If the `using` directive is missing, then when we add it, we'll
            // attempt to keep the existing statements in alphabetical order.
            for (int i = 0; i < usings.Count; i++)
            {
                // Type aliases are usually put last, so if we haven't found an
                // insertion index yet, then we can insert it before this statement.
                if (usings[i].Alias is not null)
                {
                    if (!insertionIndex.HasValue)
                    {
                        insertionIndex = i;
                    }
                }
                else
                {
                    string name = usings[i].Name.ToString();
                    // If the namespace is already imported, then we can return
                    // the original list of `using` directives without modifying them.
                    if (string.Equals(name, namespaceToImport, System.StringComparison.Ordinal))
                    {
                        return usings;
                    }

                    // If we don't have an insertion index, and this `using` directive is
                    // greater than the one we want to insert, then this is the first
                    // directive that should appear after the one we insert.
                    if (!insertionIndex.HasValue && string.Compare(name, namespaceToImport) > 0)
                    {
                        insertionIndex = i;
                    }
                }
            }

            UsingDirectiveSyntax directive = SyntaxFactory.UsingDirective(namespaceName);

            // If we found where to insert the new directive, then insert
            // it at that index; otherwise, it must be greater than all
            // existing directives, so add it to the end of the list.
            if (insertionIndex.HasValue)
            {
                return usings.Insert(insertionIndex.Value, directive);
            }
            else
            {
                return usings.Add(directive);
            }
        }

    }
}
