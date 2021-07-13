using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Community.VisualStudio.Toolkit.Shared.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using VSSDK.TestExtension;

namespace TestExtension.Commands
{
    [Command(PackageIds.TestDependencyInjection)]
    internal sealed class DependencyInjectionCommand : BaseCommand<DependencyInjectionCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IToolkitServiceProvider<TestExtensionPackage> serviceProvider = await VS.GetRequiredServiceAsync<SToolkitServiceProvider<TestExtensionPackage>, IToolkitServiceProvider<TestExtensionPackage>>();

            await VS.MessageBox.ShowAsync($"The {nameof(IToolkitServiceProvider<TestExtensionPackage>)} was retrieved from the service collection succuessfully!");
        }
    }
}
