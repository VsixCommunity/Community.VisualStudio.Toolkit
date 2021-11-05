using System;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Microsoft.VisualStudio.Shell.Interop
{
    /// <summary>
    /// Extension methods for the <see cref="IVsHierarchyExtensions"/> interface.
    /// </summary>
    public static class IVsHierarchyExtensions
    {
        /// <summary>
        /// Tries to get a property from a hierarchy item.
        /// </summary>
        /// <remarks>
        /// Inspired by https://github.com/dotnet/roslyn/blob/main/src/VisualStudio/Core/Def/Implementation/ProjectSystem/Extensions/IVsHierarchyExtensions.cs
        /// </remarks>
        public static bool TryGetItemProperty<T>(this IVsHierarchy hierarchy, uint itemId, int propertyId, out T? value)
        {
            return HierarchyUtilities.TryGetHierarchyProperty<T>(hierarchy, itemId, propertyId, out value);
        }

        /// <summary>
        /// Converts a <see cref="IVsHierarchy"/> to a <see cref="IVsHierarchyItem"/>.
        /// </summary>
        public static async Task<IVsHierarchyItem> ToHierarchyItemAsync(this IVsHierarchy hierarchy, uint itemId)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            IVsHierarchyItemManager manager = await VS.GetMefServiceAsync<IVsHierarchyItemManager>();
            return manager.GetHierarchyItem(hierarchy, itemId);
        }

        /// <summary>
        /// Converts a <see cref="IVsHierarchy"/> to a <see cref="IVsHierarchyItem"/>.
        /// </summary>
        public static IVsHierarchyItem ToHierarchyItem(this IVsHierarchy hierarchy, uint itemId)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            IVsHierarchyItemManager manager = VS.GetMefService<IVsHierarchyItemManager>();
            return manager.GetHierarchyItem(hierarchy, itemId);
        }

        /// <summary>
        /// Returns whether the specified <see cref="IVsHierarchy"/> is an 'SDK' style project.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        public static bool IsSdkStyleProject(this IVsHierarchy hierarchy)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            return hierarchy.IsCapabilityMatch("CPS");
        }

        /// <summary>Check what kind the project is.</summary>
        /// <param name="hierarchy">The hierarchy instance to check.</param>
        /// <param name="typeGuid">Use the <see cref="ProjectTypes"/> list of strings.</param>
        public static bool IsProjectOfType(this IVsHierarchy hierarchy, string typeGuid)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            if (hierarchy is IVsAggregatableProject aggregatable)
            {
                if (ErrorHandler.Succeeded(aggregatable.GetAggregateProjectTypeGuids(out string types)))
                {
                    Guid guid = new(typeGuid);

                    foreach (string type in types.Split(';'))
                    {
                        if (Guid.TryParse(type, out Guid identifier) && guid.Equals(identifier))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the <see cref="IVsSharedAssetsProject"/> for the <see cref="IVsHierarchy"/>.
        /// </summary>
        /// <param name="hierarchy"></param>
        public static IVsSharedAssetsProject? GetSharedAssetsProject(this IVsHierarchy hierarchy)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            HierarchyUtilities.TryGetHierarchyProperty<IVsSharedAssetsProject>(
                hierarchy,
                (uint)VSConstants.VSITEMID.Root,
                (int)__VSHPROPID7.VSHPROPID_SharedAssetsProject,
                out IVsSharedAssetsProject? sharedAssetsProject);

            return sharedAssetsProject;
        }

        /// <summary>
        /// Returns whether the <see cref="IVsHierarchy"/> is a shared project.
        /// </summary>
        /// <param name="hierarchy"></param>
        public static bool IsSharedProject(this IVsHierarchy hierarchy)
        {
            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            return hierarchy.GetSharedAssetsProject() != null;
        }
    }
}
