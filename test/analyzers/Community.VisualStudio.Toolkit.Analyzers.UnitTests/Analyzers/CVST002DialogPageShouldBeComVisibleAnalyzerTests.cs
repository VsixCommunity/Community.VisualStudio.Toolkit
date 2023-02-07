using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public class CVST002DialogPageShouldBeComVisibleAnalyzerTests : AnalyzerAndCodeFixTestBase<CVST002DialogPageShouldBeComVisibleAnalyzer, CVST002DialogPageShouldBeComVisibleCodeFixProvider>
    {
        public CVST002DialogPageShouldBeComVisibleAnalyzerTests()
        {
            AddReference(typeof(DialogPage));
            AddReference(typeof(ComVisibleAttribute));
        }

        [Fact]
        public async Task DoesNotFlagDialogPageImplementationsThatAreAlreadyComVisibleAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ComVisible(true)]
class Foo : DialogPage {}
";

            await VerifyAsync();
        }

        [Fact]
        public async Task DetectsMissingComVisibleAttributeAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;

class {|#0:Foo|} : DialogPage {}
";

            Expect(Diagnostic(Diagnostics.DialogPageShouldBeComVisible).WithLocation(0));

            FixedCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ComVisible(true)]
class Foo : DialogPage {}
";

            await VerifyAsync();
        }

        [Fact]
        public async Task DetectsMissingComVisibleAttributeWhenClassIndirectlyInheritsDialogPageAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

class {|#0:Foo|} : Bar {}

[ComVisible(true)]
class Bar : DialogPage {}
";

            Expect(Diagnostic(Diagnostics.DialogPageShouldBeComVisible).WithLocation(0));

            FixedCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ComVisible(true)]
class Foo : Bar {}

[ComVisible(true)]
class Bar : DialogPage {}
";

            await VerifyAsync();
        }

        [Fact]
        public async Task CodeFixDoesNotImportNamespaceIfNamespaceIsAlreadyImportedAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

class {|#0:Foo|} : DialogPage { }
";

            Expect(Diagnostic(Diagnostics.DialogPageShouldBeComVisible).WithLocation(0));

            FixedCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ComVisible(true)]
class Foo : DialogPage { }
";

            await VerifyAsync();
        }

        [Fact]
        public async Task CodeFixCanAddAttributeWhenClassContainsOtherAttributesAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using System;

[Serializable]
class {|#0:Foo|} : DialogPage { }
";

            Expect(Diagnostic(Diagnostics.DialogPageShouldBeComVisible).WithLocation(0));

            FixedCode = @"
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

[Serializable]
[ComVisible(true)]
class Foo : DialogPage { }
";

            await VerifyAsync();
        }

        [Fact]
        public async Task CodeFixAddsTheAttributeAfterXmlDocumentationAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;

/// <summary>Test class.</summary>
class {|#0:Foo|} : DialogPage { }
";

            Expect(Diagnostic(Diagnostics.DialogPageShouldBeComVisible).WithLocation(0));

            FixedCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

/// <summary>Test class.</summary>
[ComVisible(true)]
class Foo : DialogPage { }
";

            await VerifyAsync();
        }
    }
}
