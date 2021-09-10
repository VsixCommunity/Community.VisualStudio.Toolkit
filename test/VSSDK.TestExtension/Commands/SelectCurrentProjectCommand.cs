using System.Collections.Generic;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    [Command(PackageIds.SelectCurrentProject)]
    internal sealed class SelectCurrentProjectCommand : BaseCommand<SelectCurrentProjectCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SolutionExplorerWindow solutionExplorer = await VS.Windows.GetSolutionExplorerWindowAsync();
            if (solutionExplorer != null)
            {
                List<SolutionItem> projects = new List<SolutionItem>();
                foreach (SolutionItem item in await solutionExplorer.GetSelectionAsync())
                {
                    SolutionItem project = item.FindParent(SolutionItemType.Project);
                    if (project != null)
                    {
                        projects.Add(project);
                    }
                }

                if (projects.Count > 0)
                {
                    solutionExplorer.SetSelection(projects);
                }
            }
        }
    }
}
