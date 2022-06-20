using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace Community.VisualStudio.Toolkit.Analyzers.UnitTests.Analyzers
{
    public class CVST005InitializeCommandsAnalyzerTests : TestBase<CVST005InitializeCommandsAnalyzer, CVST005InitializeCommandsCodeFixProvider>
    {
        private const string _usings = @"
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading;
using System.Threading.Tasks;";

        public CVST005InitializeCommandsAnalyzerTests()
        {
            AddReference(typeof(BaseCommand<>));
            AddReference(typeof(AsyncPackage));
            AddReference(typeof(ServiceProgressData));
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenCommandsIsInitializedDirectlyAsync()
        {
            TestCode = _usings + @"

class MyCommand : BaseCommand<MyCommand> { }

class MyPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await MyCommand.InitializeAsync(this);
    }
}
";

            await VerifyAsync();
        }

        [Fact]
        public async Task DoesNotReportDiagnosticWhenCommandsAreInitializedInBulkAsync()
        {
            TestCode = _usings + @"

class MyCommand : BaseCommand<MyCommand> { }

class MyPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await this.RegisterCommandsAsync();
    }
}
";

            await VerifyAsync();
        }

        [Fact]
        public async Task ReportsDiagnosticWhenNoCommandsAreInitializedAsync()
        {
            TestCode = _usings + @"

class MyFirstCommand : BaseCommand<MyFirstCommand> { }
class MySecondCommand : BaseCommand<MySecondCommand> { }

class MyPackage : AsyncPackage
{
    protected override async Task {|#0:{|#1:InitializeAsync|}|}(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        System.Diagnostics.Debug.WriteLine(""Initializing"");
    }
}
";

            FixedCode = _usings + @"

class MyFirstCommand : BaseCommand<MyFirstCommand> { }
class MySecondCommand : BaseCommand<MySecondCommand> { }

class MyPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        System.Diagnostics.Debug.WriteLine(""Initializing"");
        await this.RegisterCommandsAsync();
    }
}
";

            Expect(
                Diagnostic(Diagnostics.InitializeCommands)
                .WithLocation(0)
                .WithArguments("MyFirstCommand"));

            Expect(
                Diagnostic(Diagnostics.InitializeCommands)
                .WithLocation(1)
                .WithArguments("MySecondCommand"));

            await VerifyAsync();
        }

        [Fact]
        public async Task ReportsDiagnosticWhenSomeCommandsAreInitializedAsync()
        {
            TestCode = _usings + @"

class MyFirstCommand : BaseCommand<MyFirstCommand> { }
class MySecondCommand : BaseCommand<MySecondCommand> { }

class MyPackage : AsyncPackage
{
    protected override async Task {|#0:InitializeAsync|}(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await MyFirstCommand.InitializeAsync(this);
    }
}
";

            FixedCode = _usings + @"

class MyFirstCommand : BaseCommand<MyFirstCommand> { }
class MySecondCommand : BaseCommand<MySecondCommand> { }

class MyPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await MyFirstCommand.InitializeAsync(this);
        await MySecondCommand.InitializeAsync(this);
    }
}
";

            Expect(
                Diagnostic(Diagnostics.InitializeCommands)
                .WithLocation(0)
                .WithArguments("MySecondCommand"));

            await VerifyAsync();
        }

        [Fact]
        public async Task ReportsDiagnosticWhenInitializeAsyncMethodDoesNotExistAsync()
        {
            TestCode = _usings + @"

class MyCommand : BaseCommand<MyCommand> { }

class {|#0:MyPackage|} : AsyncPackage
{
}
";

            FixedCode = _usings + @"

class MyCommand : BaseCommand<MyCommand> { }

class MyPackage : AsyncPackage
{
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await this.RegisterCommandsAsync();
    }
}
";

            Expect(
                Diagnostic(Diagnostics.InitializeCommands)
                .WithLocation(0)
                .WithArguments("MyCommand"));

            await VerifyAsync();
        }
    }
}
