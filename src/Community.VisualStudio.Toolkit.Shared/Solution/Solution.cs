using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft;
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
        /// Get the current solution.
        /// </summary>
        public async Task<SolutionItem?> GetCurrentSolutionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsHierarchy solution = (IVsHierarchy)await VS.Services.GetSolutionAsync();
            IVsHierarchyItem? hierItem = await solution.ToHierarcyItemAsync(VSConstants.VSITEMID_ROOT);
            return SolutionItem.FromHierarchyItem(hierItem);
        }

        /// <summary>
        /// Get the current solution.
        /// </summary>
        public SolutionItem? GetCurrentSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsHierarchy solution = (IVsHierarchy)ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution));
            Assumes.Present(solution);
            IVsHierarchyItem? hierItem = solution.ToHierarcyItem(VSConstants.VSITEMID_ROOT);
            return SolutionItem.FromHierarchyItem(hierItem);
        }

        /// <summary>
        /// Gets the active solution item.
        /// </summary>
        public async Task<SolutionItem?> GetActiveSolutionItemAsync()
        {
            IEnumerable<IVsHierarchyItem>? hierarchies = await VS.Selection.GetSelectedHierarchiesAsync();
            IVsHierarchyItem? hierarchy = hierarchies.FirstOrDefault();

            if (hierarchy != null)
            {
                return await SolutionItem.FromHierarchyAsync(hierarchy.HierarchyIdentity.NestedHierarchy, VSConstants.VSITEMID_ROOT);
            }

            return null;
        }

        /// <summary>
        /// Gets the active project.
        /// </summary>
        public async Task<SolutionItem?> GetActiveProjectAsync()
        {
            SolutionItem? item = await GetActiveSolutionItemAsync();

            if (item?.Type == SolutionItemType.Project)
            {
                return item;
            }

            return item?.FindParent(SolutionItemType.Project);
        }

        /// <summary>
        /// Get all projects in the solution.
        /// </summary>
        public async Task<IEnumerable<IVsHierarchy>> GetAllProjectHierarchiesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution? sol = await VS.Services.GetSolutionAsync();
            return sol.GetAllProjectHierarchys();
        }

        /// <summary>
        /// Gets all projects in the solution
        /// </summary>
        public async Task<IEnumerable<SolutionItem>> GetAllProjectsAsync(bool includeSolutionFolders = false)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionAsync();
            IEnumerable<IVsHierarchy>? hierarchies = solution.GetAllProjectHierarchys();

            List<SolutionItem> list = new();

            foreach (IVsHierarchy? hierarchy in hierarchies)
            {
                SolutionItem? proj = await SolutionItem.FromHierarchyAsync(hierarchy, VSConstants.VSITEMID_ROOT);

                if (proj?.Type == SolutionItemType.Project)
                {
                    list.Add(proj);
                }
                else if (includeSolutionFolders && proj?.Type == SolutionItemType.SolutionFolder)
                {
                    list.Add(proj);
                }
            }

            return list;
        }

        /// <summary>
        /// Checks if a solution is open.
        /// </summary>
        public async Task<bool> IsOpenAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionAsync();
            return solution?.IsOpen() == true;
        }

        /// <summary>
        /// Checks if a solution is openign.
        /// </summary>
        public async Task<bool> IsOpeningAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionAsync();
            return solution?.IsOpening() == true;
        }
    }
}
