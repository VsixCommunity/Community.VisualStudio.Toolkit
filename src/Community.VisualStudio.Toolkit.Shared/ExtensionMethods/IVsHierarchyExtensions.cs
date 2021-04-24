using System;
using EnvDTE;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.Shell.Interop
{
    /// <summary>
    /// Extension methods for the <see cref="IVsHierarchyExtensions"/> interface.
    /// </summary>
    public static class IVsHierarchyExtensions
    {
        /// <summary>
        /// Converts an IVsHierarchy to a Project.
        /// </summary>
        public static Project? ToProject(this IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out var obj);

            return obj as Project;
        }

        /// <summary>
        /// Returns whether the specified <see cref="IVsHierarchy"/> is an 'SDK' style project.
        /// </summary>
        /// <param name="vsHierarchy"></param>
        /// <returns></returns>
        public static bool IsSdkStyleProject(this IVsHierarchy vsHierarchy)
        {
            if (vsHierarchy == null)
                throw new ArgumentNullException(nameof(vsHierarchy));

            return vsHierarchy.IsCapabilityMatch("CPS");
        }

        /// <summary>
        /// Returns the <see cref="IVsSharedAssetsProject"/> for the <see cref="IVsHierarchy"/>.
        /// </summary>
        /// <param name="vsHierarchy"></param>
        /// <returns></returns>
        public static IVsSharedAssetsProject? GetSharedAssetsProject(this IVsHierarchy vsHierarchy)
        {
            if (vsHierarchy == null)
                throw new ArgumentNullException(nameof(vsHierarchy));

            ThreadHelper.ThrowIfNotOnUIThread();

            HierarchyUtilities.TryGetHierarchyProperty<IVsSharedAssetsProject>(
                vsHierarchy,
                (uint)VSConstants.VSITEMID.Root,
                (int)__VSHPROPID7.VSHPROPID_SharedAssetsProject,
                out var sharedAssetsProject);

            return sharedAssetsProject;
        }

        /// <summary>
        /// Returns whether the <see cref="IVsHierarchy"/> is a shared project.
        /// </summary>
        /// <param name="vsHierarchy"></param>
        /// <returns></returns>
        public static bool IsSharedProject(this IVsHierarchy vsHierarchy)
        {
            if (vsHierarchy == null)
                throw new ArgumentNullException(nameof(vsHierarchy));

            ThreadHelper.ThrowIfNotOnUIThread();

            return vsHierarchy.GetSharedAssetsProject() != null;
        }
    }
}
