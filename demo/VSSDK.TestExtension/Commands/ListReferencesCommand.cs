using System.Linq;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension.Commands
{
    [Command(PackageIds.ListReferences)]
    internal sealed class ListReferencesCommand : BaseCommand<ListReferencesCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            OutputWindowPane pane = await VS.Windows.CreateOutputWindowPaneAsync("References");
            await pane.ActivateAsync();

            foreach (Project project in await VS.Solutions.GetAllProjectsAsync())
            {
                await pane.WriteLineAsync(project.Name);
                foreach (Reference reference in project.References.OrderBy(x => x.Name))
                {
                    if (reference is AssemblyReference assemblyRef)
                    {
                        await pane.WriteLineAsync($"  * {reference.Name} (Assembly: {assemblyRef.FullPath})");
                    }
                    else if (reference is ProjectReference projectRef)
                    {
                        await pane.WriteLineAsync($"  * {reference.Name} (Project: {(await projectRef.GetProjectAsync())?.Name ?? "?"})");
                    }
                    else
                    {
                        await pane.WriteLineAsync($"  * {reference.Name} (Unknown)");
                    }
                }
            }
        }
    }
}
