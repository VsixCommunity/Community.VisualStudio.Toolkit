using System.Collections.Generic;

namespace Microsoft.VisualStudio.Shell.Interop
{
    /// <summary>
    /// Extension methods for the <see cref="IVsSharedAssetsProjectExtensions"/> interface.
    /// </summary>
    public static class IVsSharedAssetsProjectExtensions
    {
        /// <summary>
        /// Returns a collection of hierarchies that refernce the shared project
        /// </summary>
        /// <param name="sharedAssetsProject"></param>
        /// <returns></returns>
        public static IEnumerable<IVsHierarchy> GetReferencingHierarchies(this IVsSharedAssetsProject sharedAssetsProject)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (IVsHierarchy importingProject in sharedAssetsProject.EnumImportingProjects())
            {
                yield return importingProject;
            }
        }
    }
}
