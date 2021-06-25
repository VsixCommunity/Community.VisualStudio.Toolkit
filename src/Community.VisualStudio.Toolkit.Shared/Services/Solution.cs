using System;
using System.Collections.Generic;
using System.Linq;
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
        public Task<IVsSolution> GetSolutionAsync() => VS.GetRequiredServiceAsync<SVsSolution, IVsSolution>();

        /// <summary>
        /// Opens a Solution or Project using the standard open dialog boxes.
        /// </summary>
        public Task<IVsOpenProjectOrSolutionDlg> GetOpenProjectOrSolutionDlgAsync() => VS.GetRequiredServiceAsync<SVsOpenProjectOrSolutionDlg, IVsOpenProjectOrSolutionDlg>();

        /// <summary>
        /// Gets a list of the selected items.
        /// </summary>
        public async Task<IEnumerable<SelectedItem>> GetSelectedItemsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2 dte = await VS.GetRequiredServiceAsync<SDTE, DTE2>();
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

            IVsMonitorSelection? monitorSelection = await VS.GetRequiredServiceAsync<SVsShellMonitorSelection, IVsMonitorSelection>();
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
        /// Gets the currently selected hierarchy items.
        /// </summary>
        public async Task<IEnumerable<IVsHierarchyItem>> GetSelectedHierarchiesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection? svc = await VS.GetRequiredServiceAsync<SVsShellMonitorSelection, IVsMonitorSelection>();
            IntPtr hierPtr = IntPtr.Zero;
            IntPtr containerPtr = IntPtr.Zero;

            List<IVsHierarchyItem> results = new();

            try
            {
                svc.GetCurrentSelection(out hierPtr, out var itemId, out IVsMultiItemSelect multiSelect, out containerPtr);

                if (itemId == VSConstants.VSITEMID_SELECTION)
                {
                    multiSelect.GetSelectionInfo(out var itemCount, out var fSingleHierarchy);

                    var items = new VSITEMSELECTION[itemCount];
                    multiSelect.GetSelectedItems(0, itemCount, items);

                    foreach (VSITEMSELECTION item in items)
                    {
                        IVsHierarchyItem? hierItem = await item.pHier.ToHierarcyItemAsync(item.itemid);
                        if (hierItem != null && !results.Contains(hierItem))
                        {
                            results.Add(hierItem);
                        }
                    }
                }
                else
                {
                    if (hierPtr != IntPtr.Zero)
                    {
                        var hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierPtr);
                        IVsHierarchyItem? hierItem = await hierarchy.ToHierarcyItemAsync(itemId);

                        if (hierItem != null)
                        {
                            results.Add(hierItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ex.LogAsync();
            }
            finally
            {
                if (hierPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierPtr);
                }

                if (containerPtr != IntPtr.Zero)
                {
                    Marshal.Release(containerPtr);
                }
            }

            return results;
        }

        /// <summary>
        /// Gets the currently selected nodes.
        /// </summary>
        public async Task<IEnumerable<ItemNode>> GetSelectedNodesAsync()
        {
            IEnumerable<IVsHierarchyItem>? hierarchies = await GetSelectedHierarchiesAsync();
            List<ItemNode> nodes = new();

            foreach (IVsHierarchyItem hierarchy in hierarchies)
            {
                ItemNode? node = await ItemNode.CreateAsync(hierarchy.HierarchyIdentity.Hierarchy, hierarchy.HierarchyIdentity.ItemID);

                if (node != null)
                {
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        /// <summary>
        /// Gets the currently selected node.
        /// </summary>
        public async Task<ItemNode?> GetActiveProjectNodeAsync()
        {
            IEnumerable<IVsHierarchyItem>? hierarchies = await GetSelectedHierarchiesAsync();
            IVsHierarchyItem? hierarchy = hierarchies.FirstOrDefault();

            if (hierarchy != null)
            {
                return await ItemNode.CreateAsync(hierarchy.HierarchyIdentity.NestedHierarchy, VSConstants.VSITEMID_ROOT);
            }

            return null;
        }

        /// <summary>
        /// Gets all projects in the solution
        /// </summary>
        public async Task<IEnumerable<ItemNode>> GetAllProjectNodesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await GetSolutionAsync();
            IEnumerable<IVsHierarchy>? hierarchies = solution.GetAllProjectHierarchys();

            List<ItemNode> list = new();

            foreach (IVsHierarchy? hierarchy in hierarchies)
            {
                ItemNode? proj = await ItemNode.CreateAsync(hierarchy, VSConstants.VSITEMID_ROOT);

                if (proj?.Type == NodeType.Project)
                {
                    list.Add(proj);
                }
            }

            return list;
        }

        /// <summary> 
        /// Gets the active project.
        /// </summary>
        public async Task<Project?> GetActiveProjectAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2? dte = await VS.GetRequiredServiceAsync<SDTE, DTE2>();

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

        /// <summary>
        /// Builds the solution asynchronously
        /// </summary>
        /// <returns>Returns 'true' if successfull</returns>
        public async Task<bool> BuildAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            DTE2 dte = await VS.GetDTEAsync();
            return await dte.Solution.BuildAsync();
        }
    }
}
