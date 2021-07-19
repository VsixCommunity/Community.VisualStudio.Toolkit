using System;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.Shared.ExtensionMethods;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using TestExtension;
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
    public sealed class TestExtensionPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // Tool windows
            this.RegisterToolWindows();

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Commands
            await this.RegisterCommandsAsync();

            VS.Events.DocumentEvents.AfterDocumentWindowHide += DocumentEvents_AfterDocumentWindowHide;
            VS.Events.DocumentEvents.BeforeDocumentWindowShow += DocumentEvents_BeforeDocumentWindowShow;
        }

        private void DocumentEvents_BeforeDocumentWindowShow(DocumentView obj)
        {
            VS.StatusBar.ShowMessageAsync(obj.Document?.FilePath).FireAndForget();
        }

        private void DocumentEvents_AfterDocumentWindowHide(DocumentView obj)
        {
            VS.StatusBar.ShowMessageAsync(obj.Document?.FilePath).FireAndForget();
        }
    }
}
