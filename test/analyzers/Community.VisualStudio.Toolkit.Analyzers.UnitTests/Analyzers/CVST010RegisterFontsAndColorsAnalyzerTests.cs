using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public class CVST010RegisterFontsAndColorsAnalyzerTests : AnalyzerAndCodeFixTestBase<CVST010RegisterFontsAndColorsAnalyzer, CVST010RegisterFontsAndColorsCodeFixProvider>
    {
        private const string _usings = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;";

        public CVST010RegisterFontsAndColorsAnalyzerTests()
        {
            AddReference(typeof(BaseFontAndColorProvider));
            AddReference(typeof(AsyncPackage));
            AddReference(typeof(RegistrationAttribute));
            AddReference(typeof(ServiceProgressData));
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenThePackageDoesNotProvideFontsAndColorsAsync()
        {
            TestCode = _usings + @"

class MyPackage : AsyncPackage { }
";

            await VerifyAsync();
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenThePackageProvidesAndRegistersFontsAndColorsAsync()
        {
            TestCode = _usings + @"

[ProvideFontsAndColors(typeof(MyProvider))]
class MyPackage : AsyncPackage
{
    protected async override Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await this.RegisterFontAndColorProvidersAsync();
    }
}

[Guid(""4d1fa26a-668a-45f1-bb71-1a24c7b6f53e"")]
class MyProvider : BaseFontAndColorProvider { }
";

            await VerifyAsync();
        }

        [Fact]
        public async Task ReportsDiagnosticWhenThePackageProvidesFontsAndColorsButDoesNotRegisterThemAsync()
        {
            TestCode = _usings + @"

[ProvideFontsAndColors(typeof(MyProvider))]
class MyPackage : AsyncPackage
{
    protected override async Task {|#0:InitializeAsync|}(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        System.Diagnostics.Debug.WriteLine(""Initializing"");
    }
}

[Guid(""d145ca7a-36e1-4c9f-9948-d3b66139099f"")]
class MyProvider : BaseFontAndColorProvider { }
";

            FixedCode = _usings + @"

[ProvideFontsAndColors(typeof(MyProvider))]
class MyPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        System.Diagnostics.Debug.WriteLine(""Initializing"");
        await this.RegisterFontAndColorProvidersAsync();
    }
}

[Guid(""d145ca7a-36e1-4c9f-9948-d3b66139099f"")]
class MyProvider : BaseFontAndColorProvider { }
";

            Expect(Diagnostic(Diagnostics.RegisterFontsAndColors).WithLocation(0));

            await VerifyAsync();
        }

        [Fact]
        public async Task ReportsDiagnosticWhenInitializeAsyncMethodDoesNotExistAsync()
        {
            TestCode = _usings + @"

[ProvideFontsAndColors(typeof(MyProvider))]
class {|#0:MyPackage|} : AsyncPackage
{ }

[Guid(""aafc3d02-1439-45fc-8ee6-a40219c1613b"")]
class MyProvider : BaseFontAndColorProvider { }
";

            FixedCode = _usings + @"

[ProvideFontsAndColors(typeof(MyProvider))]
class MyPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await this.RegisterFontAndColorProvidersAsync();
    }
}

[Guid(""aafc3d02-1439-45fc-8ee6-a40219c1613b"")]
class MyProvider : BaseFontAndColorProvider { }
";

            Expect(Diagnostic(Diagnostics.RegisterFontsAndColors).WithLocation(0));

            await VerifyAsync();
        }
    }
}
