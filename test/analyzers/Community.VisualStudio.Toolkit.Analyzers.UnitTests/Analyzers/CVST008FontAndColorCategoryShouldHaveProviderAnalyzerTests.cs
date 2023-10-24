using System.Threading.Tasks;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public class CVST008FontAndColorCategoryShouldHaveProviderAnalyzerTests : AnalyzerTestBase<CVST008FontAndColorCategoryShouldHaveProviderAnalyzer>
    {
        private const string _usings = @"
using Community.VisualStudio.Toolkit;
using System.Runtime.InteropServices;";

        public CVST008FontAndColorCategoryShouldHaveProviderAnalyzerTests()
        {
            AddReference(typeof(BaseFontAndColorProvider));
            AddReference(typeof(BaseFontAndColorCategory<>));
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenCategoryAndProviderExistAsync()
        {
            TestCode = _usings + @"

[Guid(""467e4911-7031-476a-9b36-898aa2f8e6d8"")]
class MyProvider : BaseFontAndColorProvider { }

[Guid(""a7a5f88f-1dfc-4aea-ab60-e60c29330b01"")]
class MyCategory : BaseFontAndColorCategory<MyCategory> {
    public override string Name => ""My category"";
}
";

            await VerifyAsync();
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenNoCategoriesOrProvidersExistAsync()
        {
            TestCode = _usings + @"

class MyClass { }
";

            await VerifyAsync();
        }

        [Fact]
        public async Task ReportsDiagnosticWhenCategoriesExistButProviderDoeNotAsync()
        {
            TestCode = _usings + @"

[Guid(""c1b1be18-6d82-4eeb-a126-6fee991597ef"")]
class {|#0:FirstCategory|} : BaseFontAndColorCategory<FirstCategory> {
    public override string Name => ""First category"";
}

[Guid(""f262a118-de65-4d51-af4d-7953d3d3082c"")]
class {|#1:SecondCategory|} : BaseFontAndColorCategory<SecondCategory> {
    public override string Name => ""Second category"";
}
";

            Expect(Diagnostic(Diagnostics.DefineProviderForFontAndColorCategories).WithLocation(0));
            Expect(Diagnostic(Diagnostics.DefineProviderForFontAndColorCategories).WithLocation(1));

            await VerifyAsync();
        }
    }
}
