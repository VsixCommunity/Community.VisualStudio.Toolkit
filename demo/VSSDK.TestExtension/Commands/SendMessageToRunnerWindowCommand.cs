using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace TestExtension
{
    [Command(PackageIds.SendMessageToRunnerWindow)]
    internal sealed class SendMessageToRunnerWindowCommand : BaseCommand<SendMessageToRunnerWindowCommand>
    {
        private int _counter;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            RunnerWindowMessenger messenger = await Package.GetServiceAsync<RunnerWindowMessenger, RunnerWindowMessenger>();
            _counter += 1;
            messenger.Send($"Message #{_counter}");
        }
    }
}
