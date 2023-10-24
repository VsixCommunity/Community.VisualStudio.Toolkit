using System.Threading.Tasks;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public class CVST007SpecifyFontAndColorCategoryGuidAnalyzerTests : AnalyzerAndCodeFixTestBase<CVST007SpecifyFontAndColorCategoryGuidAnalyzer, CVST007SpecifyFontAndColorCategoryGuidCodeFixProvider>
    {
        private const string _usings = @"
using Community.VisualStudio.Toolkit;
using System.Runtime.InteropServices;";

        public CVST007SpecifyFontAndColorCategoryGuidAnalyzerTests()
        {
            AddReference(typeof(BaseFontAndColorCategory<>));
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenCategoryHasGuidAsync()
        {
            TestCode = _usings + @"

[Guid(""a7a5f88f-1dfc-4aea-ab60-e60c29330b01"")]
class MyCategory : BaseFontAndColorCategory<MyCategory> {
    public override string Name => ""My category"";
}
";

            await VerifyAsync();
        }

        [Fact]
        public async Task ReportsDiagnosticWhenCategoryDoesNotHaveGuidAsync()
        {
            TestCode = _usings + @"

class {|#0:MyCategory|} : BaseFontAndColorCategory<MyCategory> {
    public override string Name => ""My category"";
}
";

            // Note: We can't test the code fix because it generates a new GUID each time,
            // which means we don't know exactly what the fixed code will look like.
            Expect(Diagnostic(Diagnostics.DefineExplicitGuidForFontAndColorCategories).WithLocation(0));

            await VerifyAsync();
        }
    }
}
