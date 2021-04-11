using System;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    internal sealed class RunnerWindowCommand : BaseCommand<RunnerWindowCommand>
    {
        public RunnerWindowCommand() : base(new Guid("cb765f49-fc35-4c14-93af-bb48ca4f2ce3"), 0x0100)
        { }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await RunnerWindow.ShowAsync();
        }
    }
}
