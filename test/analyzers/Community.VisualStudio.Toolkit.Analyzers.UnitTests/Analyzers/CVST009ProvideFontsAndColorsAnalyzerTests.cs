using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public class CVST009ProvideFontsAndColorsAnalyzerTests : AnalyzerTestBase<CVST009ProvideFontsAndColorsAnalyzer>
    {
        private const string _usings = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;";

        public CVST009ProvideFontsAndColorsAnalyzerTests()
        {
            AddReference(typeof(BaseFontAndColorProvider));
            AddReference(typeof(AsyncPackage));
            AddReference(typeof(RegistrationAttribute));
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenThereAreNoProvidersAsync()
        {
            TestCode = _usings + @"

class MyPackage : AsyncPackage { }
";

            await VerifyAsync();
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenThereIsNoPackageAsync()
        {
            TestCode = _usings + @"

[Guid(""306763cd-ac5f-41a6-8f7d-20e2777053fa"")]
class MyProvider : BaseFontAndColorProvider { }
";

            await VerifyAsync();
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenAllProvidersAreDeclaredOnPackageAsync()
        {
            TestCode = _usings + @"

[ProvideFontsAndColors(typeof(AProvider))]
class FirstPackage : AsyncPackage { }

[ProvideFontsAndColors(typeof(BProvider))]
[ProvideFontsAndColors(typeof(CProvider))]
class SecondPackage : AsyncPackage { }

[Guid(""3f88236f-8d6d-42c9-a9d3-39fdd375e6e3"")]
class AProvider : BaseFontAndColorProvider { }

[Guid(""d707dfc5-48b8-4916-ac06-af5b323fa1c0"")]
class BProvider : BaseFontAndColorProvider { }

[Guid(""b877e47c-fad5-4e5f-a743-80c662f4fcec"")]
class CProvider : BaseFontAndColorProvider { }
";

            await VerifyAsync();
        }

        [Fact]
        public async Task ReportsDiagnosticOnProvidersThatAreNotDeclaredOnPackageAsync()
        {
            TestCode = _usings + @"

[ProvideFontsAndColors(typeof(FirstProvider))]
class {|#0:{|#1:FirstPackage|}|} : AsyncPackage { }

[ProvideFontsAndColors(typeof(SecondProvider))]
class SecondPackage : AsyncPackage { }

[Guid(""b3e3eb3d-547a-412b-835b-cb2b2b91a832"")]
class FirstProvider : BaseFontAndColorProvider { }

[Guid(""eda2423e-c50b-45ee-b441-91da97a7dc27"")]
class SecondProvider : BaseFontAndColorProvider { }

[Guid(""99eb85fc-f4be-4645-b1f1-3691451ae015"")]
class ThirdCategory : BaseFontAndColorProvider { }

[Guid(""0a802693-1871-4919-9664-51b101c20fe2"")]
class FourthCategory : BaseFontAndColorProvider { }
";

            Expect(Diagnostic(Diagnostics.ProvideFontsAndColors).WithLocation(0).WithArguments("ThirdCategory"));
            Expect(Diagnostic(Diagnostics.ProvideFontsAndColors).WithLocation(1).WithArguments("FourthCategory"));

            await VerifyAsync();
        }
    }
}
