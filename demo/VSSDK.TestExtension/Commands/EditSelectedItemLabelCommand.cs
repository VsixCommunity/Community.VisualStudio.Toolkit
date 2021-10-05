using System.Linq;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    [Command(PackageIds.EditSelectedItemLabel)]
    internal sealed class EditSelectedItemLabelCommand : BaseCommand<EditSelectedItemLabelCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SolutionExplorerWindow solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
            if (solutionExplorer != null)
            {
                SolutionItem item = (await solutionExplorer.GetSelectionAsync()).FirstOrDefault();
                if (item != null)
                {
                    solutionExplorer.EditLabel(item);
                }
            }
        }
    }
}
