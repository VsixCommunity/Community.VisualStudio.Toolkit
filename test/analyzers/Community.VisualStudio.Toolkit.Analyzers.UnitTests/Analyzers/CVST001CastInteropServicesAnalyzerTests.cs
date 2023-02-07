using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests
{
    public class CVST001CastInteropServicesAnalyzerTests : AnalyzerAndCodeFixTestBase<CVST001CastInteropServicesAnalyzer, CVST001CastInteropServicesCodeFixProvider>
    {
        /// <summary>
        /// Using the real IVsImageService2 and other interop types requires adding a reference to the interop assembly, 
        /// and that's too complicated for these simple tests. We'll just define the interfaces ourselves.
        /// </summary>
        private const string _interopTypeDefinitions = @"namespace Microsoft.VisualStudio.Shell.Interop 
            {
                public interface IVsImageService2 { }
            }";

        [Fact]
        public async Task DetectsInteropServiceWithoutCastAsync()
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo 
    {
    public async Task Bar() 
    {
        var service = {|#0:await VS.Services.GetImageServiceAsync()|};
    }
}
" + _interopTypeDefinitions;

            Expect(
                Diagnostic(Diagnostics.CastInteropTypes)
                .WithLocation(0)
                .WithArguments("Microsoft.VisualStudio.Shell.Interop.IVsImageService2"));

            FixedCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo 
    {
    public async Task Bar() 
    {
        var service = (IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Fact]
        public async Task DetectsMissingCastInConfigureAwaitCallAsync()
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        var service = {|#0:await VS.Services.GetImageServiceAsync().ConfigureAwait(false)|};
    }
}
" + _interopTypeDefinitions;

            Expect(
                Diagnostic(Diagnostics.CastInteropTypes)
                .WithLocation(0)
                .WithArguments("Microsoft.VisualStudio.Shell.Interop.IVsImageService2"));

            FixedCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        var service = (IVsImageService2)await VS.Services.GetImageServiceAsync().ConfigureAwait(false);
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Fact]
        public async Task DetectsIncorrectCastAsync()
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        var service = {|#0:(string)await VS.Services.GetImageServiceAsync()|};
    }
}
" + _interopTypeDefinitions;

            Expect(
                Diagnostic(Diagnostics.CastInteropTypes)
                .WithLocation(0)
                .WithArguments("Microsoft.VisualStudio.Shell.Interop.IVsImageService2"));

            FixedCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        var service = (IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Theory]
        [MemberData(nameof(GetStatementsToFix))]
        public async Task CodeFixDoesNotImportNamespaceIfNamespaceIsAlreadyImportedAsync(string statement)
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        var service = {|#0:" + statement + @"|};
    }
}
" + _interopTypeDefinitions;

            Expect(
                Diagnostic(Diagnostics.CastInteropTypes)
                .WithLocation(0)
                .WithArguments("Microsoft.VisualStudio.Shell.Interop.IVsImageService2"));

            FixedCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        var service = (IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Fact]
        public async Task AcceptsCorrectCastUsingFullNameAsync()
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        var service = (Microsoft.VisualStudio.Shell.Interop.IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;


            await VerifyAsync();
        }

        [Fact]
        public async Task AcceptsCorrectCastWithImportedNamespaceAsync()
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        var service = (IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Fact]
        public async Task AcceptsServiceThatDoesNotRequireCastAsync()
        {
            AddReference(typeof(Microsoft.VisualStudio.ComponentModelHost.IComponentModel2));

            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        var service = await VS.Services.GetComponentModelAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Fact]
        public async Task DoesNotFlagCallsThatAreNotAwaitedAsync()
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo 
    {
    public async Task Bar() 
    {
        var service = VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Theory]
        [MemberData(nameof(GetStatementsToFix))]
        public async Task ChangesVariableTypeFromObjectWhenServiceIsAssignedToVariableDeclarationAsync(string statement)
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        object service = {|#0:" + statement + @"|};
    }
}
" + _interopTypeDefinitions;

            Expect(
                Diagnostic(Diagnostics.CastInteropTypes)
                .WithLocation(0)
                .WithArguments("Microsoft.VisualStudio.Shell.Interop.IVsImageService2"));

            FixedCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        IVsImageService2 service = (IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Theory]
        [MemberData(nameof(GetStatementsToFix))]
        public async Task ChangesVariableTypeFromObjectWhenServiceIsAssignedToVariableSeparateFromDeclarationAsync(string statement)
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        object service;
        service = {|#0:" + statement + @"|};
    }
}
" + _interopTypeDefinitions;

            Expect(
                Diagnostic(Diagnostics.CastInteropTypes)
                .WithLocation(0)
                .WithArguments("Microsoft.VisualStudio.Shell.Interop.IVsImageService2"));

            FixedCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar()
    {
        IVsImageService2 service;
        service = (IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Theory]
        [MemberData(nameof(GetStatementsToFix))]
        public async Task ChangesFieldTypeFromObjectWhenServiceIsAssignedToFieldAsync(string statement)
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo
{
    private object service;
    public async Task Bar()
    {
        service = {|#0:" + statement + @"|};
    }
}
" + _interopTypeDefinitions;

            Expect(
                Diagnostic(Diagnostics.CastInteropTypes)
                .WithLocation(0)
                .WithArguments("Microsoft.VisualStudio.Shell.Interop.IVsImageService2"));

            FixedCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    private IVsImageService2 service;
    public async Task Bar()
    {
        service = (IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Theory]
        [MemberData(nameof(GetStatementsToFix))]
        public async Task DoesNotChangeParameterTypeFromObjectWhenServiceIsAssignedToParameterAsync(string statement)
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar(object service)
    {
        service = {|#0:" + statement + @"|};
    }
}
" + _interopTypeDefinitions;

            Expect(
                Diagnostic(Diagnostics.CastInteropTypes)
                .WithLocation(0)
                .WithArguments("Microsoft.VisualStudio.Shell.Interop.IVsImageService2"));

            FixedCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar(object service)
    {
        service = (IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }

        [Theory]
        [MemberData(nameof(GetStatementsToFix))]
        public async Task DoesNotChangeParameterTypeFromObjectWhenServiceIsAssignedToPropertyAsync(string statement)
        {
            TestCode = @"
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar(object service)
    {
        Service = {|#0:" + statement + @"|};
    }
    public object Service { get; set; }
}
" + _interopTypeDefinitions;

            Expect(
                Diagnostic(Diagnostics.CastInteropTypes)
                .WithLocation(0)
                .WithArguments("Microsoft.VisualStudio.Shell.Interop.IVsImageService2"));

            FixedCode = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

class Foo
{
    public async Task Bar(object service)
    {
        Service = (IVsImageService2)await VS.Services.GetImageServiceAsync();
    }
    public object Service { get; set; }
}
" + _interopTypeDefinitions;

            await VerifyAsync();
        }


        public static IEnumerable<object[]> GetStatementsToFix()
        {
            yield return new[] { "await VS.Services.GetImageServiceAsync()" };
            yield return new[] { "(string)await VS.Services.GetImageServiceAsync()" };
        }
    }
}
