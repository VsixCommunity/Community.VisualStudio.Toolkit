using System.Linq;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension.Commands
{
    [Command(PackageIds.ListReferences)]
    internal sealed class ListReferencesCommand : BaseCommand<ListReferencesCommand>
    {
        OutputWindowPane _pane;

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_pane is null)
            {
                _pane = await VS.Windows.CreateOutputWindowPaneAsync("References");
            }

            await _pane.ActivateAsync();

            foreach (Project project in await VS.Solutions.GetAllProjectsAsync())
            {
                await _pane.WriteLineAsync(project.Name);
                foreach (Reference reference in project.References.OrderBy(x => x.Name))
                {
                    if (reference is AssemblyReference assemblyRef)
                    {
                        await _pane.WriteLineAsync($"  * {reference.Name} (Assembly: {assemblyRef.FullPath})");
                    }
                    else if (reference is ProjectReference projectRef)
                    {
                        await _pane.WriteLineAsync($"  * {reference.Name} (Project: {(await projectRef.GetProjectAsync())?.Name ?? "?"})");
                    }
                    else
                    {
                        await _pane.WriteLineAsync($"  * {reference.Name} (Unknown)");
                    }
                }
            }
        }
    }
}
