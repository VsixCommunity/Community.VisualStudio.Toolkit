using System.Threading.Tasks;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public class CVST006SpecifyFontAndColorProviderGuidAnalyzerTests : AnalyzerAndCodeFixTestBase<CVST006SpecifyFontAndColorProviderGuidAnalyzer, CVST006SpecifyFontAndColorProviderGuidCodeFixProvider>
    {
        private const string _usings = @"
using Community.VisualStudio.Toolkit;
using System.Runtime.InteropServices;";

        public CVST006SpecifyFontAndColorProviderGuidAnalyzerTests()
        {
            AddReference(typeof(BaseFontAndColorProvider));
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenProviderHasGuidAsync()
        {
            TestCode = _usings + @"

[Guid(""e91c7045-4768-453d-97f0-88f39fa0ad8f"")]
class MyProvider : BaseFontAndColorProvider { }
";

            await VerifyAsync();
        }

        [Fact]
        public async Task ReportsDiagnosticWhenProviderDoesNotHaveGuidAsync()
        {
            TestCode = _usings + @"

class {|#0:MyProvider|} : BaseFontAndColorProvider { }
";

            // Note: We can't test the code fix because it generates a new GUID each time,
            // which means we don't know exactly what the fixed code will look like.
            Expect(Diagnostic(Diagnostics.DefineExplicitGuidForFontAndColorProviders).WithLocation(0));

            await VerifyAsync();
        }
    }
}
