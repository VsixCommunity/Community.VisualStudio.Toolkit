using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public abstract class TestBase<TAnalyzer> where TAnalyzer : DiagnosticAnalyzer, new()
    {
        private bool _verified;

        protected static string NormalizeLineEndings(string value)
        {
            // The code may contain different line endings to what is expected depending on how
            // it was checked out in Git. Change the line endings to match the current environment,
            // because that is the type of line ending that will be used by the code fix.
            return Regex.Replace(value, "\r?\n", Environment.NewLine);
        }

        protected DiagnosticResult Diagnostic() => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic();

        protected DiagnosticResult Diagnostic(string diagnosticId) => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);

        protected DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(descriptor);

        protected Task VerifyAsync()
        {
            // Flag that the test has verified something to guard against a test 
            // that wasn't written correctly and failed to verify anything.
            _verified = true;
            return RunTestAsync();
        }

        protected abstract Task RunTestAsync();

        protected virtual void Dispose(bool disposing)
        {
            // Prevent tests from silently passing because they didn't verify anything.
            if (!_verified)
            {
                throw new InvalidOperationException("The test did not verify anything.");
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected static Microsoft.CodeAnalysis.Solution ConfigureProject(
            Microsoft.CodeAnalysis.Solution solution,
            ProjectId projectId,
            List<Type> references
        )
        {
            Microsoft.CodeAnalysis.Project? project = solution.GetProject(projectId)
                ?? throw new InvalidOperationException("Project is null.");

            CompilationOptions? options = project.CompilationOptions
                ?? throw new InvalidOperationException("The project does not have compilation options.");

            options = options.WithSpecificDiagnosticOptions(
                options.SpecificDiagnosticOptions.SetItems(VerifierHelper.NullableWarnings)
            );

            solution = solution.WithProjectCompilationOptions(projectId, options);

            foreach (Type reference in references)
            {
                solution = solution.AddMetadataReference(
                    projectId,
                    MetadataReference.CreateFromFile(reference.Assembly.Location)
                );
            }

            return solution;
        }
    }
}
