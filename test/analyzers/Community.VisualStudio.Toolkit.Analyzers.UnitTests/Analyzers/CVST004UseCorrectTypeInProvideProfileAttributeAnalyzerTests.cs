using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public class CVST004UseCorrectTypeInProvideProfileAttributeAnalyzerTests : TestBase<CVST004UseCorrectTypeInProvideProfileAttributeAnalyzer, CVST004UseCorrectTypeInProvideProfileAttributeCodeFixProvider>
    {
        private const string _profileManagerImplementationBody = @"
            public void SaveSettingsToXml(IVsSettingsWriter writer) {}
            public void LoadSettingsFromXml(IVsSettingsReader reader) {}
            public void SaveSettingsToStorage() {}
            public void LoadSettingsFromStorage() {}
            public void ResetSettings() {}
            ";

        public CVST004UseCorrectTypeInProvideProfileAttributeAnalyzerTests()
        {
            AddReference(typeof(IProfileManager));
            AddReference(typeof(RegistrationAttribute));
            AddReference(typeof(ProvideProfileAttribute));
            AddReference(typeof(IVsSettingsReader));
        }

        [Fact]
        public async Task DoesNotFlagWhenProfileManagerIsUsedInAttributeAsync()
        {
            TestCode = $@"
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;

[ProvideProfile(typeof(MyProfile), ""Foo"", ""Bar"", 0, 0, false)]
class Package {{}}
class MyProfile : IProfileManager {{
    {_profileManagerImplementationBody}
}}
";

            await VerifyAsync();
        }

        [Fact]
        public async Task FlagsIncorrectTypeInAttributeAsync()
        {
            TestCode = @"
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;

[ProvideProfile(typeof({|#0:MyProfile|}), ""Foo"", ""Bar"", 0, 0, false)]
class Package {}
class MyProfile {}
";

            Expect(
                Diagnostic(Diagnostics.UseCorrectTypeInProvideProfileAttribute)
                    .WithLocation(0)
                    .WithMessage("The type 'MyProfile' is not assignable to 'Microsoft.VisualStudio.Shell.IProfileManager'")
            );

            await VerifyAsync();
        }

        [Theory]
        [InlineData(0, "MyProfile.Alpha")]
        [InlineData(1, "MyProfile.Beta")]
        [InlineData(2, "MyProfile.Gamma")]
        public async Task CanFixIncorrectTypeWhenSpecifiedTypeContainsNestedDialogPageTypesAsync(int codeFixIndex, string expectedTypeName)
        {
            TestCode = $@"
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;

[ProvideProfile(typeof({{|#0:MyProfile|}}), ""Foo"", ""Bar"", 0, 0, false)]
class Package {{}}
class MyProfile {{
    public class Alpha : IProfileManager {{
        {_profileManagerImplementationBody}
    }}
    public class Gamma : IProfileManager {{
        {_profileManagerImplementationBody}
    }}
    public class Beta : IProfileManager {{
        {_profileManagerImplementationBody}
    }}
}}
";

            Expect(
                Diagnostic(Diagnostics.UseCorrectTypeInProvideProfileAttribute)
                    .WithLocation(0)
                    .WithMessage("The type 'MyProfile' is not assignable to 'Microsoft.VisualStudio.Shell.IProfileManager'")
            );

            CodeActionIndex = codeFixIndex;
            CodeActionVerifier = (action, verifier) => verifier.Equal(action.Title, $"Change to '{expectedTypeName}'");

            FixedCode = $@"
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;

[ProvideProfile(typeof({expectedTypeName}), ""Foo"", ""Bar"", 0, 0, false)]
class Package {{}}
class MyProfile {{
    public class Alpha : IProfileManager {{
        {_profileManagerImplementationBody}
    }}
    public class Gamma : IProfileManager {{
        {_profileManagerImplementationBody}
    }}
    public class Beta : IProfileManager {{
        {_profileManagerImplementationBody}
    }}
}}
";

            await VerifyAsync();
        }
    }
}
