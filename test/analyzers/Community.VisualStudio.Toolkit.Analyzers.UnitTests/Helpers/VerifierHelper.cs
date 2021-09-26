using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    internal static class VerifierHelper
    {
        internal static ImmutableDictionary<string, ReportDiagnostic> NullableWarnings { get; } = GetNullableWarningsFromCompiler();

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = { "/warnaserror:nullable" };
            CSharpCommandLineArguments commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
            ImmutableDictionary<string, ReportDiagnostic> nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

            // Workaround for https://github.com/dotnet/roslyn/issues/41610
            nullableWarnings = nullableWarnings
                .SetItem("CS8632", ReportDiagnostic.Error)
                .SetItem("CS8669", ReportDiagnostic.Error);

            return nullableWarnings;
        }
    }
}
