using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension.Commands
{
    [Command(PackageIds.BuildSolutionAsync)]
    internal sealed class BuildSolutionAsyncCommand : BaseCommand<BuildSolutionAsyncCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var buildResult = await VS.Build.BuildSolutionAsync();
            if (buildResult)
                VS.Notifications.ShowMessage("Build Result", $"The solution was built successfully!");
            else
                VS.Notifications.ShowError("Build Result", $"The solution did not build successfully :(");
        }
    }
}
