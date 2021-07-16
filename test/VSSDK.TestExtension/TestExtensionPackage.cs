using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.DependencyInjection.Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using TestExtension;
using TestExtension.Commands;
using Task = System.Threading.Tasks.Task;

namespace VSSDK.TestExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.TestExtensionString)]
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), nameof(TestExtension), "General", 0, 0, true)]
    [ProvideProfile(typeof(OptionsProvider.GeneralOptions), nameof(TestExtension), "General", 0, 0, true)]
    [ProvideToolWindow(typeof(RunnerWindow.Pane), Style = VsDockStyle.Float, Window = WindowGuids.SolutionExplorer)]
    [ProvideToolWindowVisibility(typeof(RunnerWindow.Pane), VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideToolWindow(typeof(ThemeWindow.Pane))]
    [ProvideFileIcon(".abc", "KnownMonikers.Reference")]
    [ProvideToolWindow(typeof(MultiInstanceWindow.Pane))]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class TestExtensionPackage : MicrosoftDIToolkitPackage<TestExtensionPackage>
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            // Tool windows
            RunnerWindow.Initialize(this);
            ThemeWindow.Initialize(this);
            MultiInstanceWindow.Initialize(this);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Commands
            await RunnerWindowCommand.InitializeAsync(this);
            await ThemeWindowCommand.InitializeAsync(this);
            await MultiInstanceWindowCommand.InitializeAsync(this);
            await BuildActiveProjectAsyncCommand.InitializeAsync(this);
            await BuildSolutionAsyncCommand.InitializeAsync(this);
            await DependencyInjectionCommand.InitializeAsync(this);
        }
    }
}
