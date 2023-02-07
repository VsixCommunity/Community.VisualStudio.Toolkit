using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public class CVST003UseCorrectTypeInProvideOptionDialogPageAttributeAnalyzerTests : AnalyzerAndCodeFixTestBase<CVST003UseCorrectTypeInProvideOptionDialogPageAttributeAnalyzer, CVST003UseCorrectTypeInProvideOptionDialogPageAttributeCodeFixProvider>
    {
        public CVST003UseCorrectTypeInProvideOptionDialogPageAttributeAnalyzerTests()
        {
            AddReference(typeof(DialogPage));
            AddReference(typeof(RegistrationAttribute));
            AddReference(typeof(ProvideOptionPageAttribute));
        }

        [Fact]
        public async Task DoesNotFlagWhenDialogPageIsUsedInAttributeAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ProvideOptionPage(typeof(MyDialogPage), ""Foo"", ""Bar"", 0, 0, false, 0)]
class Package {}
class MyDialogPage : DialogPage {}
";

            await VerifyAsync();
        }

        [Fact]
        public async Task FlagsIncorrectTypeInProvideOptionPageAttributeAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ProvideOptionPage(typeof({|#0:MyDialogPage|}), ""Foo"", ""Bar"", 0, 0, false, 0)]
class Package {}
class MyDialogPage {}
";

            Expect(
                Diagnostic(Diagnostics.UseCorrectTypeInProvideOptionDialogPageAttribute)
                    .WithLocation(0)
                    .WithMessage("The type 'MyDialogPage' is not assignable to 'Microsoft.VisualStudio.Shell.DialogPage'")
            );

            await VerifyAsync();
        }

        [Fact]
        public async Task FlagsIncorrectTypeInProvideToolboxPageAttributeAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ProvideToolboxPage(typeof({|#0:MyDialogPage|}), 0)]
class Package {}
class MyDialogPage {}
";

            Expect(
                Diagnostic(Diagnostics.UseCorrectTypeInProvideOptionDialogPageAttribute)
                    .WithLocation(0)
                    .WithMessage("The type 'MyDialogPage' is not assignable to 'Microsoft.VisualStudio.Shell.DialogPage'")
            );

            await VerifyAsync();
        }

        [Fact]
        public async Task FlagsIncorrectTypeInProvideLanguageEditorOptionPageAttributeAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ProvideLanguageEditorOptionPage(typeof({|#0:MyDialogPage|}), ""a"", ""b"", ""c"", ""d"")]
class Package {}
class MyDialogPage {}
";

            Expect(
                Diagnostic(Diagnostics.UseCorrectTypeInProvideOptionDialogPageAttribute)
                    .WithLocation(0)
                    .WithMessage("The type 'MyDialogPage' is not assignable to 'Microsoft.VisualStudio.Shell.DialogPage'")
            );

            await VerifyAsync();
        }

        [Theory]
        [InlineData(0, "MyDialogPage.Alpha")]
        [InlineData(1, "MyDialogPage.Beta")]
        [InlineData(2, "MyDialogPage.Gamma")]
        public async Task CanFixIncorrectTypeWhenSpecifiedTypeContainsNestedDialogPageTypesAsync(int codeFixIndex, string expectedTypeName)
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ProvideOptionPage(typeof({|#0:MyDialogPage|}), ""Foo"", ""Bar"", 0, 0, false, 0)]
class Package {}
class MyDialogPage {
    public class Alpha : DialogPage {}
    public class Gamma : DialogPage {}
    public class Beta : DialogPage {}
}
";

            Expect(
                Diagnostic(Diagnostics.UseCorrectTypeInProvideOptionDialogPageAttribute)
                    .WithLocation(0)
                    .WithMessage("The type 'MyDialogPage' is not assignable to 'Microsoft.VisualStudio.Shell.DialogPage'")
            );

            CodeActionIndex = codeFixIndex;
            CodeActionVerifier = (action, verifier) => verifier.Equal(action.Title, $"Change to '{expectedTypeName}'");

            FixedCode = $@"
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

[ProvideOptionPage(typeof({expectedTypeName}), ""Foo"", ""Bar"", 0, 0, false, 0)]
class Package {{}}
class MyDialogPage {{
    public class Alpha : DialogPage {{}}
    public class Gamma : DialogPage {{}}
    public class Beta : DialogPage {{}}
}}
";

            await VerifyAsync();
        }
    }
}
