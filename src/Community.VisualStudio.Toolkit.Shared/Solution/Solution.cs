using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
        public Task<IVsSolution> GetSolutionServiceAsync() => VS.GetRequiredServiceAsync<SVsSolution, IVsSolution>();


        /// <summary>
        /// Opens a Solution or Project using the standard open dialog boxes.
        /// </summary>
        public Task<IVsOpenProjectOrSolutionDlg> GetOpenProjectOrSolutionDlgAsync() => VS.GetRequiredServiceAsync<SVsOpenProjectOrSolutionDlg, IVsOpenProjectOrSolutionDlg>();

        /// <summary>
        /// Gets the currently selected hierarchy items.
        /// </summary>
        public async Task<IEnumerable<IVsHierarchyItem>> GetSelectedHierarchiesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection? svc = await VS.Selection.GetMonitorSelectionAsync();
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
                else if (hierPtr != IntPtr.Zero)
                {
                    var hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierPtr);
                    IVsHierarchyItem? hierItem = await hierarchy.ToHierarcyItemAsync(itemId);

                    if (hierItem != null)
                    {
                        results.Add(hierItem);
                    }
                }
                else if (await GetSolutionServiceAsync() is IVsHierarchy solution)
                {
                    IVsHierarchyItem? sol = await solution.ToHierarcyItemAsync(VSConstants.VSITEMID_ROOT);
                    if (sol != null)
                    {
                        results.Add(sol);
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
        public async Task<IEnumerable<SolutionItem>> GetSelectedNodesAsync()
        {
            IEnumerable<IVsHierarchyItem>? hierarchies = await GetSelectedHierarchiesAsync();
            List<SolutionItem> nodes = new();

            foreach (IVsHierarchyItem hierarchy in hierarchies)
            {
                SolutionItem? node = await SolutionItem.FromHierarchyAsync(hierarchy.HierarchyIdentity.Hierarchy, hierarchy.HierarchyIdentity.ItemID);

                if (node != null)
                {
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        /// <summary>
        /// Get the current solution.
        /// </summary>
        public async Task<SolutionItem?> GetCurrentSolutionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var solution = (IVsHierarchy)await GetSolutionServiceAsync();
            IVsHierarchyItem? hierItem = await solution.ToHierarcyItemAsync(VSConstants.VSITEMID_ROOT);
            return SolutionItem.FromHierarchyItem(hierItem);
        }

        /// <summary>
        /// Gets the currently selected node.
        /// </summary>
        public async Task<SolutionItem?> GetActiveProjectNodeAsync()
        {
            IEnumerable<IVsHierarchyItem>? hierarchies = await GetSelectedHierarchiesAsync();
            IVsHierarchyItem? hierarchy = hierarchies.FirstOrDefault();

            if (hierarchy != null)
            {
                return await SolutionItem.FromHierarchyAsync(hierarchy.HierarchyIdentity.NestedHierarchy, VSConstants.VSITEMID_ROOT);
            }

            return null;
        }

        /// <summary>
        /// Get all projects in the solution.
        /// </summary>
        public async Task<IEnumerable<IVsHierarchy>> GetAllProjectHierarchiesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution? sol = await GetSolutionServiceAsync();
            return sol.GetAllProjectHierarchys();
        }

        /// <summary>
        /// Gets all projects in the solution
        /// </summary>
        public async Task<IEnumerable<SolutionItem>> GetAllProjectNodesAsync(bool includeSolutionFolders = false)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await GetSolutionServiceAsync();
            IEnumerable<IVsHierarchy>? hierarchies = solution.GetAllProjectHierarchys();

            List<SolutionItem> list = new();

            foreach (IVsHierarchy? hierarchy in hierarchies)
            {
                SolutionItem? proj = await SolutionItem.FromHierarchyAsync(hierarchy, VSConstants.VSITEMID_ROOT);

                if (proj?.Type == NodeType.Project)
                {
                    list.Add(proj);
                }
                else if (includeSolutionFolders && proj?.Type == NodeType.SolutionFolder)
                {
                    list.Add(proj);
                }
            }

            return list;
        }
    }
}
