using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A collection of services and helpers related to solutions.
    /// </summary>
    public class Solutions
    {
        internal Solutions()
        { }

        /// <summary>
        /// Gets the current solution.
        /// </summary>
        public async Task<Solution?> GetCurrentSolutionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsHierarchy solution = (IVsHierarchy)await VS.Services.GetSolutionAsync();
            IVsHierarchyItem? hierItem = await solution.ToHierarchyItemAsync(VSConstants.VSITEMID_ROOT);
            return SolutionItem.FromHierarchyItem(hierItem) as Solution;
        }

        /// <summary>
        /// Gets the current solution.
        /// </summary>
        public Solution? GetCurrentSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsHierarchy solution = VS.GetRequiredService<SVsSolution, IVsHierarchy>();
            IVsHierarchyItem? hierItem = solution.ToHierarchyItem(VSConstants.VSITEMID_ROOT);
            return SolutionItem.FromHierarchyItem(hierItem) as Solution;
        }

        /// <summary>
        /// Gets the active project.
        /// </summary>
        public async Task<Project?> GetActiveProjectAsync()
        {
            SolutionItem? item = await GetActiveItemAsync();

            if (item == null)
            {
                return null;
            }

            if (item.Type == SolutionItemType.Project)
            {
                return item as Project;
            }

            return item.FindParent(SolutionItemType.Project) as Project;
        }

        /// <summary>
        /// Gets all projects in the solution.
        /// </summary>
        public async Task<IEnumerable<IVsHierarchy>> GetAllProjectHierarchiesAsync(ProjectStateFilter filter = ProjectStateFilter.Loaded)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution sol = await VS.Services.GetSolutionAsync();
            return sol.GetAllProjectHierarchies(filter);
        }

        /// <summary>
        /// Gets all projects in the solution
        /// </summary>
        public async Task<IEnumerable<Project>> GetAllProjectsAsync(ProjectStateFilter filter = ProjectStateFilter.Loaded)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionAsync();
            IEnumerable<IVsHierarchy> hierarchies = solution.GetAllProjectHierarchies(filter);

            List<Project> list = new();

            foreach (IVsHierarchy hierarchy in hierarchies)
            {
                Project? proj = await SolutionItem.FromHierarchyAsync(hierarchy, VSConstants.VSITEMID_ROOT) as Project;

                if (proj?.Type == SolutionItemType.Project)
                {
                    list.Add(proj);
                }
            }

            return list;
        }


        /// <summary>
        /// Gets the currently selected items.
        /// </summary>
        public async Task<IEnumerable<SolutionItem>> GetActiveItemsAsync()
        {
            IEnumerable<IVsHierarchyItem>? hierarchies = await GetSelectedHierarchiesAsync();
            List<SolutionItem> items = new();

            foreach (IVsHierarchyItem hierarchy in hierarchies)
            {
                SolutionItem? item = await SolutionItem.FromHierarchyAsync(hierarchy.HierarchyIdentity.Hierarchy, hierarchy.HierarchyIdentity.ItemID);

                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        /// Gets the currently selected item. If more than one item is selected, it returns the first one.
        /// </summary>
        /// <remarks><see langword="null"/> if no items are selected.</remarks>
        public async Task<SolutionItem?> GetActiveItemAsync()
        {
            IEnumerable<SolutionItem>? items = await GetActiveItemsAsync();
            return items?.FirstOrDefault();
        }

        /// <summary>
        /// Checks if a solution is open.
        /// </summary>
        public async Task<bool> IsOpenAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionAsync();
            return solution.IsOpen() == true;
        }

        /// <summary>
        /// Checks if a solution is opening.
        /// </summary>
        public async Task<bool> IsOpeningAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionAsync();
            return solution.IsOpening() == true;
        }

        /// <summary>
        /// Gets the currently selected hierarchy items.
        /// </summary>
        private async Task<IEnumerable<IVsHierarchyItem>> GetSelectedHierarchiesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection svc = await VS.Services.GetMonitorSelectionAsync();
            IntPtr hierPtr = IntPtr.Zero;
            IntPtr containerPtr = IntPtr.Zero;

            List<IVsHierarchyItem> results = new();

            try
            {
                svc.GetCurrentSelection(out hierPtr, out uint itemId, out IVsMultiItemSelect multiSelect, out containerPtr);
                await AddHierarchiesFromSelectionAsync(hierPtr, itemId, multiSelect, results);
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

        internal static async Task AddHierarchiesFromSelectionAsync(IntPtr hierPtr, uint itemId, IVsMultiItemSelect? multiSelect, List<IVsHierarchyItem> hierarchies)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (itemId == VSConstants.VSITEMID_SELECTION && multiSelect is not null)
            {
                multiSelect.GetSelectionInfo(out uint itemCount, out int _);

                VSITEMSELECTION[] items = new VSITEMSELECTION[itemCount];
                multiSelect.GetSelectedItems(0, itemCount, items);

                hierarchies.Capacity = (int)itemCount;

                foreach (VSITEMSELECTION item in items)
                {
                    IVsHierarchyItem? hierItem = await item.pHier.ToHierarchyItemAsync(item.itemid);

                    if (hierItem != null)
                    {
                        hierarchies.Add(hierItem);
                    }
                    else
                    {
                        IVsHierarchy solution = (IVsHierarchy)await VS.Services.GetSolutionAsync();
                        IVsHierarchyItem? sol = await solution.ToHierarchyItemAsync(VSConstants.VSITEMID_ROOT);

                        if (sol != null)
                        {
                            hierarchies.Add(sol);
                        }
                    }
                }
            }
            else if (itemId == VSConstants.VSITEMID_NIL)
            {
                // Empty Solution Explorer or nothing selected, so don't add anything.
            }
            else if (hierPtr != IntPtr.Zero)
            {
                IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierPtr);
                IVsHierarchyItem? hierItem = await hierarchy.ToHierarchyItemAsync(itemId);

                if (hierItem != null)
                {
                    hierarchies.Add(hierItem);
                }
            }
            else if (await VS.Services.GetSolutionAsync() is IVsHierarchy solution)
            {
                IVsHierarchyItem? sol = await solution.ToHierarchyItemAsync(VSConstants.VSITEMID_ROOT);

                if (sol != null)
                {
                    hierarchies.Add(sol);
                }
            }
        }
    }
}
