using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Community.VisualStudio.Toolkit.Analyzers
{
    public abstract class AnalyzerBase : DiagnosticAnalyzer
    {
        protected static LocalizableString GetLocalizableString(string name)
        {
            return new LocalizableResourceString(name, Resources.ResourceManager, typeof(Resources));
        }

        protected static ImmutableDictionary<string, string> CreateProperties(string key, string value)
        {
            ImmutableDictionary<string, string>.Builder builder = ImmutableDictionary.CreateBuilder<string, string>();
            builder.Add(key, value);
            return builder.ToImmutable();
        }
    }
}
