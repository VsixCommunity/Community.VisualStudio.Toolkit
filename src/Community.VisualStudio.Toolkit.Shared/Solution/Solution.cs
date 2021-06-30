using System.Collections.Generic;
using System.Linq;
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
        /// Get the current solution.
        /// </summary>
        public async Task<SolutionItem?> GetCurrentSolutionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var solution = (IVsHierarchy)await VS.Services.GetSolutionServiceAsync();
            IVsHierarchyItem? hierItem = await solution.ToHierarcyItemAsync(VSConstants.VSITEMID_ROOT);
            return SolutionItem.FromHierarchyItem(hierItem);
        }

        /// <summary>
        /// Gets the currently selected node.
        /// </summary>
        public async Task<SolutionItem?> GetActiveProjectItemAsync()
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
        /// Get all projects in the solution.
        /// </summary>
        public async Task<IEnumerable<IVsHierarchy>> GetAllProjectHierarchiesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution? sol = await VS.Services.GetSolutionServiceAsync();
            return sol.GetAllProjectHierarchys();
        }

        /// <summary>
        /// Gets all projects in the solution
        /// </summary>
        public async Task<IEnumerable<SolutionItem>> GetAllProjectItemsAsync(bool includeSolutionFolders = false)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionServiceAsync();
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
