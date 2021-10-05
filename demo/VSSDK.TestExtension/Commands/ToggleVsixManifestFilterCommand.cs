using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    [Command(PackageIds.ToggleVsixManifestFilter)]
    internal sealed class ToggleVsixManifestFilterCommand : BaseCommand<ToggleVsixManifestFilterCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SolutionExplorerWindow solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
            if (solutionExplorer != null)
            {
                if (solutionExplorer.IsFilterEnabled<VsixManifestFilterProvider>())
                {
                    solutionExplorer.DisableFilter();
                }
                else
                {
                    solutionExplorer.EnableFilter<VsixManifestFilterProvider>();
                }
            }
        }
    }
}
