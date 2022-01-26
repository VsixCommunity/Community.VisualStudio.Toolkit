using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public abstract class TestBase<TAnalyzer, TCodeFix> : IDisposable
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        private readonly Test _test = new();
        private bool _verified;

        protected string TestCode
        {
            set { _test.TestCode = NormalizeLineEndings(value); }
        }

        protected string FixedCode
        {
            set { _test.FixedCode = NormalizeLineEndings(value); }
        }

        private static string NormalizeLineEndings(string value)
        {
            // The code may contain different line endings to what is expected depending on how
            // it was checked out in Git. Change the line endings to match the current environment,
            // because that is the type of line ending that that will be used by the code fix.
            return Regex.Replace(value, "\r?\n", Environment.NewLine);
        }

        protected int? CodeActionIndex
        {
            set { _test.CodeActionIndex = value; }
        }

        protected Action<CodeAction, IVerifier>? CodeActionVerifier
        {
            set { _test.CodeActionVerifier = value; }
        }

        protected void AddReference(Type typeInAssemblyToReference)
        {
            _test.References.Add(typeInAssemblyToReference);
        }

        protected void Expect(DiagnosticResult result)
        {
            _test.ExpectedDiagnostics.Add(result);
        }

        protected DiagnosticResult Diagnostic() => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic();

        protected DiagnosticResult Diagnostic(string diagnosticId) => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);

        protected DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(descriptor);

        protected async Task VerifyAsync()
        {
            // Flag that the test has verified something to guard against a test 
            // that wasn't written correctly and failed to verify anything.
            _verified = true;
            await _test.RunAsync(CancellationToken.None);
        }

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

        private class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
        {
            public Test()
            {
                References.Add(typeof(VS));

                SolutionTransforms.Add((solution, projectId) =>
                {
                    Microsoft.CodeAnalysis.Project? project = solution.GetProject(projectId);
                    if (project is null)
                    {
                        throw new InvalidOperationException("Project is null.");
                    }

                    CompilationOptions? options = project.CompilationOptions;
                    if (options is null)
                    {
                        throw new InvalidOperationException("The project does not have compilation options.");
                    }

                    options = options.WithSpecificDiagnosticOptions(
                        options.SpecificDiagnosticOptions.SetItems(VerifierHelper.NullableWarnings)
                    );

                    solution = solution.WithProjectCompilationOptions(projectId, options);

                    foreach (Type reference in References)
                    {
                        solution = solution.AddMetadataReference(projectId, MetadataReference.CreateFromFile(reference.Assembly.Location));
                    }

                    return solution;
                });
            }

            public List<Type> References { get; } = new();
        }
    }
}
