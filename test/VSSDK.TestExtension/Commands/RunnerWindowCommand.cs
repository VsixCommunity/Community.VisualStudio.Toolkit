using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    [Command(PackageGuids.CommandSetString, PackageIds.RunnerWindow)]
    internal sealed class RunnerWindowCommand : BaseCommand<RunnerWindowCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await RunnerWindow.ShowAsync();
        }
    }
}
