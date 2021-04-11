using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A collection of services and helpers related to solutions.
    /// </summary>
    public class Solution
    {
        internal Solution()
        { }

        /// <summary>
        /// Provides top-level manipulation or maintenance of the solution.
        /// </summary>
        public Task<IVsSolution> GetSolutionAsync() => VS.GetServiceAsync<SVsSolution, IVsSolution>();

        /// <summary>
        /// Opens a Solution or Project using the standard open dialog boxes.
        /// </summary>
        public Task<IVsOpenProjectOrSolutionDlg> GetOpenProjectOrSolutionDlgAsync() => VS.GetServiceAsync<SVsOpenProjectOrSolutionDlg, IVsOpenProjectOrSolutionDlg>();

        /// <summary>
        /// Gets a list of the selected items.
        /// </summary>
        public async Task<IEnumerable<SelectedItem>> GetSelectedItemsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2 dte = await VS.GetServiceAsync<SDTE, DTE2>();
            List<SelectedItem> list = new();

            foreach (SelectedItem item in dte.SelectedItems)
            {
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Returns the active <see cref="ProjectItem"/>.
        /// </summary>
        public async Task<ProjectItem?> GetActiveProjectItemAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection? monitorSelection = await VS.GetServiceAsync<SVsShellMonitorSelection, IVsMonitorSelection>();
            IntPtr hierarchyPointer = IntPtr.Zero;
            IntPtr selectionContainerPointer = IntPtr.Zero;
            object? selectedObject = null;

            try
            {
                monitorSelection.GetCurrentSelection(out hierarchyPointer,
                                                 out var itemId,
                                                 out IVsMultiItemSelect multiItemSelect,
                                                 out selectionContainerPointer);

                if (Marshal.GetTypedObjectForIUnknown(hierarchyPointer, typeof(IVsHierarchy)) is IVsHierarchy selectedHierarchy)
                {
                    ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out selectedObject));
                }
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
            finally
            {
                Marshal.Release(hierarchyPointer);
                Marshal.Release(selectionContainerPointer);
            }

            return selectedObject as ProjectItem;
        }

        /// <summary> 
        /// Gets the active project.
        /// </summary>
        public async Task<Project?> GetActiveProjectAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2? dte = await VS.GetServiceAsync<SDTE, DTE2>();

            try
            {
                if (dte.ActiveSolutionProjects is Array projects && projects.Length > 0)
                {
                    return projects.GetValue(0) as Project;
                }
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }

            return null;
        }

        /// <summary>
        /// Gets all projects int he solution
        /// </summary>
        public async Task<IEnumerable<Project>> GetAllProjectsInSolutionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await GetSolutionAsync();
            return solution.GetAllProjects();
        }

        /// <summary>
        /// Gets the directory of the currently loaded solution.
        /// </summary>
        public async Task<string?> GetSolutionDirectoryAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsSolution solution = await GetSolutionAsync();
            return solution.GetDirectory();
        }

        /// <summary>
        /// Gets the file path of the currently loaded solution file (.sln).
        /// </summary>
        public async Task<string?> GetSolutionFilePathAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsSolution solution = await GetSolutionAsync();
            return solution.GetFilePath();
        }
    }
}
