using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;

namespace TestExtension.Commands
{
    [Command(PackageIds.LoadSelectedProject)]
    internal sealed class LoadSelectedProjectCommand : BaseCommand<LoadSelectedProjectCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Project project = await VS.Solutions.GetActiveProjectAsync();
            if (project != null)
            {
                await project.LoadAsync();
                
                // Show a message to demonstrate that the Project object can still be used.
                await VS.MessageBox.ShowAsync($"Project '{project.Name}' has been loaded.");
            }
        }
    }
}
