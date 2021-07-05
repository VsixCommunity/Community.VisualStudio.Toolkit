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

            bool buildResult = await VS.Build.BuildSolutionAsync();
            if (buildResult)
            {
                await VS.MessageBox.ShowAsync("Build Result", $"The solution was built successfully!");
            }
            else
            {
                await VS.MessageBox.ShowErrorAsync("Build Result", $"The solution did not build successfully :(");
            }
        }
    }
}
