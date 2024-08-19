using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public abstract class AnalyzerAndCodeFixTestBase<TAnalyzer, TCodeFix> : TestBase<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        private readonly Test _test = new();

        protected string TestCode
        {
            set { _test.TestCode = NormalizeLineEndings(value); }
        }

        protected string FixedCode
        {
            set { _test.FixedCode = NormalizeLineEndings(value); }
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

        protected override Task RunTestAsync()
        {
            return _test.RunAsync(CancellationToken.None);
        }

        private class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
        {
            public Test()
            {
                References.Add(typeof(VS));
                SolutionTransforms.Add((solution, projectId) => ConfigureProject(solution, projectId, References));
            }

            public List<Type> References { get; } = new();
        }
    }
}
