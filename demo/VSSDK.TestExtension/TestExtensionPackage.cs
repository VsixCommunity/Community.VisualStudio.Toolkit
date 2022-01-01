using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Community.VisualStudio.Toolkit;
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
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideService(typeof(RunnerWindowMessenger), IsAsyncQueryable = true)]
    public sealed class TestExtensionPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            AddService(typeof(RunnerWindowMessenger), (_, _, _) => Task.FromResult<object>(new RunnerWindowMessenger()));

            // Tool windows
            this.RegisterToolWindows();

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Commands
            await this.RegisterCommandsAsync();

            VS.Events.DocumentEvents.AfterDocumentWindowHide += DocumentEvents_AfterDocumentWindowHide;
            VS.Events.DocumentEvents.BeforeDocumentWindowShow += DocumentEvents_BeforeDocumentWindowShow;
            VS.Events.ProjectItemsEvents.AfterRenameProjectItems += ProjectItemsEvents_AfterRenameProjectItems;
            VS.Events.ProjectItemsEvents.AfterRemoveProjectItems += ProjectItemsEvents_AfterRemoveProjectItems;
            VS.Events.SolutionEvents.OnAfterOpenProject += SolutionEvents_OnAfterOpenProject;
            VS.Events.SolutionEvents.OnBeforeOpenProject += SolutionEvents_OnBeforeOpenProject;
            VS.Events.BuildEvents.ProjectConfigurationChanged += BuildEvents_ProjectConfigurationChanged;
            VS.Events.BuildEvents.SolutionConfigurationChanged += BuildEvents_SolutionConfigurationChanged;
        }

        private void BuildEvents_SolutionConfigurationChanged()
        {
            VS.StatusBar.ShowMessageAsync("Solution configuration changed").FireAndForget();
        }

        private void BuildEvents_ProjectConfigurationChanged(Project? obj)
        {
            if (obj != null)
            {
                VS.StatusBar.ShowMessageAsync($"Configuration for project {obj.Name} changed").FireAndForget();
            }

        }

        private void SolutionEvents_OnBeforeOpenProject(string obj)
        {
            VS.StatusBar.ShowMessageAsync("About to open " + obj).FireAndForget();
        }

        private void SolutionEvents_OnAfterOpenProject(Project obj)
        {
            if (obj != null)
            {
                VS.StatusBar.ShowMessageAsync("Opened project " + obj.Name).FireAndForget();
            }
        }

        private void ProjectItemsEvents_AfterRemoveProjectItems(AfterRemoveProjectItemEventArgs obj)
        {
            string info = string.Join(",", obj.ProjectItemRemoves.Select(x => $"{x.Project.Name}:{x.RemovedItemName}"));
            VS.MessageBox.ShowConfirm(info);
        }

        private void ProjectItemsEvents_AfterRenameProjectItems(AfterRenameProjectItemEventArgs obj)
        {
            string info = string.Join(",", obj.ProjectItemRenames.Select(x => $"{x.SolutionItem.Name}:{x.OldName}"));
            VS.MessageBox.ShowConfirm(info);
        }

        private void DocumentEvents_BeforeDocumentWindowShow(DocumentView obj)
        {
            VS.StatusBar.ShowMessageAsync(obj.Document?.FilePath ?? "").FireAndForget();
        }

        private void DocumentEvents_AfterDocumentWindowHide(DocumentView obj)
        {
            VS.StatusBar.ShowMessageAsync(obj.Document?.FilePath ?? "").FireAndForget();
        }
    }
}
