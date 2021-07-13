using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.Shared.DependencyInjection;
using Microsoft.VisualStudio.Shell;

namespace TestExtension.Commands
{
    [Command(PackageIds.TestDependencyInjection)]
    internal sealed class DependencyInjectionCommand : BaseCommand<DependencyInjectionCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IToolkitServiceProvider serviceProvider = await VS.GetRequiredServiceAsync<SToolkitServiceProvider, IToolkitServiceProvider>();

            await VS.MessageBox.ShowAsync($"The {nameof(IToolkitServiceProvider)} was retrieved from the service collection succuessfully!");
        }
    }
}
