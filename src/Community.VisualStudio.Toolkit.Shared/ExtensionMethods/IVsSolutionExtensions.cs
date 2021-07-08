using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;

namespace Microsoft.VisualStudio.Shell.Interop
{
    /// <summary>
    /// Extension methods for the IVsSolution interface.
    /// </summary>
    public static class IVsSolutionExtensions
    {
        /// <summary>
        /// Checks if a solution is open.
        /// </summary>
        /// <returns><c>true</c> if a solution file is open.</returns>
        public static bool IsOpen(this IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            solution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value);
            return value is bool isOpen && isOpen;
        }

        /// <summary>
        /// Checks if a solution is opening.
        /// </summary>
        /// <returns><c>true</c> if a solution file is opening.</returns>
        public static bool IsOpening(this IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            solution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpening, out object value);
            return value is bool isOpening && isOpening;
        }

        /// <summary>
        /// Gets all projects in the solution as IVsHierarchy items.
        /// </summary>
        public static IEnumerable<IVsHierarchy> GetAllProjectHierarchys(this IVsSolution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return GetAllProjectHierarchys(solution, __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION);
        }

        /// <summary>
        /// Gets all projects in the solution as IVsHierarchy items.
        /// </summary>
        public static IEnumerable<IVsHierarchy> GetAllProjectHierarchys(this IVsSolution solution, __VSENUMPROJFLAGS flags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (solution == null)
            {
                yield break;
            }

            Guid guid = Guid.Empty;
            solution.GetProjectEnum((uint)flags, ref guid, out IEnumHierarchies enumHierarchies);
            if (enumHierarchies == null)
            {
                yield break;
            }

            IVsHierarchy[] hierarchy = new IVsHierarchy[1];
            while (enumHierarchies.Next(1, hierarchy, out uint fetched) == VSConstants.S_OK && fetched == 1)
            {
                if (hierarchy.Length > 0 && hierarchy[0] != null)
                {
                    yield return hierarchy[0];
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="SolutionItem"/> representing the solution.
        /// </summary>
        public static async Task<SolutionItem?> ToSolutionItemAsync(this IVsSolution solution)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            if (solution is IVsHierarchy hier)
            {
                IVsHierarchyItem? item = await hier.ToHierarchyItemAsync(VSConstants.VSITEMID_ROOT);
                return SolutionItem.FromHierarchyItem(item);
            }

            return null;
        }
    }
}
