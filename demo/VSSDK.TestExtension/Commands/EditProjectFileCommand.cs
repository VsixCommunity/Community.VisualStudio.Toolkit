using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace TestExtension.Commands
{
    [Command(PackageIds.EditProjectFile)]
    internal sealed class EditProjectFileCommand : BaseDynamicCommand<EditProjectFileCommand, Project>
    {
        [SuppressMessage("Usage", "VSTHRD102:Implement internal logic asynchronously", Justification = "Must be synchronous.")]
        protected override IReadOnlyList<Project> GetItems()
        {
            return ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                return (await VS.Solutions.GetAllProjectsAsync()).OrderBy(x => x.Name).ToList();
            });
        }

        protected override void BeforeQueryStatus(OleMenuCommand menuItem, EventArgs e, Project project)
        {
            menuItem.Text = project.Name;
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e, Project project)
        {
            try
            {
                await VS.Documents.OpenAsync(project.FullPath);
            }
            catch (COMException ex) when (ex.ErrorCode == -2147467259)
            {
                // The project needs to be unloaded before it
                // can be opened. Get the GUID of the project.
                project.GetItemInfo(out IVsHierarchy hierarchy, out _, out _);
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                IVsSolution solution = await VS.Services.GetSolutionAsync();
                ErrorHandler.ThrowOnFailure(solution.GetGuidOfProject(hierarchy, out Guid guid));

                // Unload the project.
                ((IVsSolution4)solution).UnloadProject(guid, (uint)_VSProjectUnloadStatus.UNLOADSTATUS_UnloadedByUser);

                // Now try to open the project file again.
                await VS.Documents.OpenAsync(project.FullPath);
            }
        }
    }

}
