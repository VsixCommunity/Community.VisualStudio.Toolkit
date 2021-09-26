using System;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace TestExtension.Commands
{
    [Command(PackageIds.BuildActiveProjectAsync)]
    internal sealed class BuildActiveProjectAsyncCommand : BaseCommand<BuildActiveProjectAsyncCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            SolutionItem activeItem = await VS.Solutions.GetActiveItemAsync();
            if (activeItem != null)
            {
                Project activeProject;
                if (activeItem.Type == SolutionItemType.Project)
                {
                    activeProject = (Project)activeItem;
                }
                else
                {
                    activeProject = activeItem.FindParent(SolutionItemType.Project) as Project;
                }

                if (activeProject != null) 
                {
                    try
                    {
                        bool buildResult = await VS.Build.BuildProjectAsync(activeProject);
                        if (buildResult)
                        {
                            await VS.MessageBox.ShowAsync("Build Result", $"The '{activeProject.Name}' project was built successfully!");
                        }
                        else
                        {
                            await VS.MessageBox.ShowErrorAsync("Build Result", $"The '{activeProject.Name}' project did not build successfully :(");
                        }

                    }
                    catch (OperationCanceledException)
                    {
                        await VS.MessageBox.ShowAsync("Build Result", $"The build of '{activeProject.Name}' was cancelled.");
                    }
                }
            }
        }
    }
}
