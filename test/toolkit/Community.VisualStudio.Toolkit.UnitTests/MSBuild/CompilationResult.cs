using System.Collections.Generic;

namespace Community.VisualStudio.Toolkit.UnitTests
{
    internal class CompilationResult
    {
        public CompilationResult(int exitCode, IEnumerable<string> standardOutput, IEnumerable<string> standardError)
        {
            ExitCode = exitCode;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }

        public int ExitCode { get; }

        public IEnumerable<string> StandardOutput { get; }

        public IEnumerable<string> StandardError { get; }
    }
}
