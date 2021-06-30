using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension.Commands
{
    [Command(PackageIds.BuildActiveProjectAsync)]
    internal sealed class BuildActiveProjectAsyncCommand : BaseCommand<BuildActiveProjectAsyncCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SolutionItem activeProject = await VS.Solution.GetActiveSolutionItemAsync();
            if (activeProject != null)
            {
                var buildResult = await VS.Build.BuildProjectAsync(activeProject);
                if (buildResult)
                {
                    await VS.MessageBox.ShowAsync("Build Result", $"The '{activeProject.Name}' project was built successfully!");
                }
                else
                {
                    await VS.MessageBox.ShowErrorAsync("Build Result", $"The '{activeProject.Name}' project did not build successfully :(");
                }
            }
        }
    }
}
