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

            var activeProject = await VS.Solution.GetActiveProjectAsync();
            if (activeProject != null)
            {
                var buildResult = await activeProject.BuildAsync();
                if (buildResult)
                    VS.Notifications.ShowMessage("Build Result", $"The '{activeProject.Name}' project was built successfully!");
                else
                    VS.Notifications.ShowError("Build Result", $"The '{activeProject.Name}' project did not build successfully :(");
            }
        }
    }
}
