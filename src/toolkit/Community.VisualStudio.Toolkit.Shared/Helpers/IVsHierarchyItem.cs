using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit.Shared.Helpers
{
    /// <summary>
    /// Extensions for the <see cref="IVsHierarchyItem"/> interface.
    /// </summary>
    public static class VsHierarchyExtensions
    {
        /// <summary>
        /// Returns the <see cref="IVsHierarchy"/> for the <see cref="IVsHierarchyItemIdentity"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IVsHierarchy GetHierarchy(this IVsHierarchyItemIdentity item) =>
            item.IsNestedItem ? item.NestedHierarchy : item.Hierarchy;

        /// <summary>
        /// Returns the 'ItemId' for the <see cref="IVsHierarchyItemIdentity"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static uint GetItemId(this IVsHierarchyItemIdentity item) =>
            item.IsNestedItem ? item.NestedItemID : item.ItemID;

        /// <summary>
        /// Returns the <see cref="IVsHierarchy"/> for the <see cref="IVsHierarchyItem"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IVsHierarchy GetHierarchy(this IVsHierarchyItem item) =>
            item.HierarchyIdentity.GetHierarchy();

        /// <summary>
        /// Returns the 'ItemId' for the <see cref="IVsHierarchyItem"/>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static uint GetItemId(this IVsHierarchyItem item) =>
            item.HierarchyIdentity.GetItemId();
    }
}
