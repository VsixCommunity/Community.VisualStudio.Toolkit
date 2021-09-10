using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    [Command(PackageIds.CollapseSelectedItems)]
    internal sealed class CollapseSelectedItemsCommand : BaseCommand<CollapseSelectedItemsCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SolutionExplorerWindow solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
            if (solutionExplorer != null)
            {
                solutionExplorer.Collapse(await solutionExplorer.GetSelectionAsync());
            }
        }
    }
}
