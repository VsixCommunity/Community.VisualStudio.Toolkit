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

            var activeProject = await VS.Solution.GetActiveProjectNodeAsync();
            if (activeProject != null)
            {
                var buildResult = await VS.Solution.BuildAsync(BuildAction.Build, activeProject);
                if (buildResult)
                    VS.Notifications.ShowMessage("Build Result", $"The '{activeProject.Name}' project was built successfully!");
                else
                    VS.Notifications.ShowError("Build Result", $"The '{activeProject.Name}' project did not build successfully :(");
            }
        }
    }
}
