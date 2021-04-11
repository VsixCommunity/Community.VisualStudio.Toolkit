using System;
using Microsoft.VisualStudio.Shell;
using Community.VisualStudio.Toolkit;
using Task = System.Threading.Tasks.Task;

namespace TestExtension
{
    public class TestCommand : BaseCommand<TestCommand>
    {
        public TestCommand()
            : base(new Guid("489ba882-f600-4c8b-89db-eb366a4ee3b3"), 0x0001) { }

        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return base.ExecuteAsync(e);
        }
    }
}
