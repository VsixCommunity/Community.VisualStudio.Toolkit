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
    public class Solutions
    {
        internal Solutions()
        { }

        /// <summary>
        /// Gets the current solution.
        /// </summary>
        public async Task<SolutionItem?> GetCurrentSolutionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsHierarchy solution = (IVsHierarchy)await VS.Services.GetSolutionAsync();
            IVsHierarchyItem? hierItem = await solution.ToHierarcyItemAsync(VSConstants.VSITEMID_ROOT);
            return SolutionItem.FromHierarchyItem(hierItem);
        }

        /// <summary>
        /// Gets the current solution.
        /// </summary>
        public SolutionItem? GetCurrentSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsHierarchy solution = VS.GetRequiredService<SVsSolution, IVsHierarchy>();
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
        public async Task<Project?> GetActiveProjectAsync()
        {
            SolutionItem? item = await GetActiveSolutionItemAsync();

            if (item?.Type == SolutionItemType.Project)
            {
                return item as Project;
            }

            return item?.FindParent(SolutionItemType.Project) as Project;
        }

        /// <summary>
        /// Gets all projects in the solution.
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
        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionAsync();
            IEnumerable<IVsHierarchy>? hierarchies = solution.GetAllProjectHierarchys();

            List<Project> list = new();

            foreach (IVsHierarchy? hierarchy in hierarchies)
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
        /// Checks if a solution is open.
        /// </summary>
        public async Task<bool> IsOpenAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionAsync();
            return solution?.IsOpen() == true;
        }

        /// <summary>
        /// Checks if a solution is opening.
        /// </summary>
        public async Task<bool> IsOpeningAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution solution = await VS.Services.GetSolutionAsync();
            return solution?.IsOpening() == true;
        }
    }
}
