using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Provides access to the Solution Explorer tool window.
    /// </summary>
    public class SolutionExplorerWindow
    {
        internal SolutionExplorerWindow(IVsWindowFrame frame, IVsUIHierarchyWindow window)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Frame = frame;
            UIHierarchyWindow = window;
            SolutionUIHierarchyWindow = (IVsSolutionUIHierarchyWindow)window;
        }

        /// <summary>
        /// Gets the Solution Explorer frame.
        /// </summary>
        public IVsWindowFrame Frame { get; }

        /// <summary>
        /// Gets the internal <see cref="IVsUIHierarchyWindow"/> that is used by Solution Explorer.
        /// </summary>
        public IVsUIHierarchyWindow UIHierarchyWindow { get; }

        /// <summary>
        /// Gets the internal <see cref="IVsSolutionUIHierarchyWindow"/> that is used by Solution Explorer.
        /// </summary>
        public IVsSolutionUIHierarchyWindow SolutionUIHierarchyWindow { get; }

        /// <summary>
        /// Determines whether Solution Explorer is currently being filtered by any filter.
        /// </summary>
        public bool IsFilterEnabled()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return SolutionUIHierarchyWindow.IsFilterEnabled();
        }

        /// <summary>
        /// Determines whether Solution Explorer is currently being filtered by the specified filter.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter to test for.</typeparam>
        public bool IsFilterEnabled<TFilter>() where TFilter : HierarchyTreeFilterProvider
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GetFilterIdentifier<TFilter>(out Guid filterGroup, out uint filterId);
            return IsFilterEnabled(filterGroup, filterId);
        }

        /// <summary>
        /// Determines whether Solution Explorer is currently being filtered by any filter.
        /// </summary>
        public bool IsFilterEnabled(Guid filterGroup, uint filterId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (IsFilterEnabled())
            {
                CommandID? current = GetCurrentFilter();
                return (current != null) && (current.Guid == filterGroup) && (current.ID == filterId);
            }

            return false;
        }

        /// <summary>
        /// Disables filtering in Solution Explorer.
        /// </summary>
        public void DisableFilter()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SolutionUIHierarchyWindow.DisableFilter();
        }

        /// <summary>
        /// Enables the specified Solution Explorer filter.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter to enable.</typeparam>
        public void EnableFilter<TFilter>() where TFilter : HierarchyTreeFilterProvider
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            GetFilterIdentifier<TFilter>(out Guid filterGroup, out uint filterId);
            EnableFilter(filterGroup, filterId);
        }

        /// <summary>
        /// Enables the specified Solution Explorer filter.
        /// </summary>
        /// <param name="filterGroup">The GUID of the filter's group.</param>
        /// <param name="filterId">The ID of the filter.</param>
        public void EnableFilter(Guid filterGroup, uint filterId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SolutionUIHierarchyWindow.EnableFilter(ref filterGroup, filterId);
        }

        /// <summary>
        /// Gets the current filter that is applied to Solution Explorer.
        /// </summary>
        public CommandID? GetCurrentFilter()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            SolutionUIHierarchyWindow.GetCurrentFilter(out Guid filterGroup, out uint filterId);
            if (filterGroup != default)
            {
                return new CommandID(filterGroup, (int)filterId);
            }
            return null;
        }

        private void GetFilterIdentifier<TFilter>(out Guid filterGroup, out uint filterId) where TFilter : HierarchyTreeFilterProvider
        {
            SolutionTreeFilterProviderAttribute? attribute = typeof(TFilter).GetCustomAttribute<SolutionTreeFilterProviderAttribute>();
            if (attribute is null)
            {
                throw new InvalidOperationException($"The type '{typeof(TFilter).FullName}' is missing the {nameof(SolutionTreeFilterProviderAttribute)} attribute.");
            }
            filterGroup = Guid.Parse(attribute.FilterCommandGroup);
            filterId = attribute.FilterCommandID;
        }

        /// <summary>
        /// Gets the selected items in Solution Explorer.
        /// </summary>
        public async Task<IEnumerable<SolutionItem>> GetSelectionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            List<IVsHierarchyItem> hierarchies = new();
            IntPtr hierPtr = IntPtr.Zero;
            IntPtr containerPtr = IntPtr.Zero;

            try
            {
                // This is similar to `IVsMonitorSelection.GetCurrentSelection()`, but when
                // there are multiple items selected, the `itemId` parameter will _not_ be
                // set to `VSITEMID_SELECTION`. Apart from that, the results are identical,
                // so we can use the same method to convert the result into IVsHierarhcy objects.
                UIHierarchyWindow.GetCurrentSelection(out hierPtr, out uint itemId, out IVsMultiItemSelect multiSelect);

                if (multiSelect is not null)
                {
                    itemId = VSConstants.VSITEMID_SELECTION;
                }

                await Solutions.AddHierarchiesFromSelectionAsync(hierPtr, itemId, multiSelect, hierarchies);
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

            List<SolutionItem> solutionItems = new();

            foreach (IVsHierarchyItem hierarchy in hierarchies)
            {
                SolutionItem? item = await SolutionItem.FromHierarchyAsync(hierarchy.HierarchyIdentity.Hierarchy, hierarchy.HierarchyIdentity.ItemID);
                if (item != null)
                {
                    solutionItems.Add(item);
                }
            }

            return solutionItems;
        }

        /// <summary>
        /// Sets the selected item in the Solution Explorer window.
        /// </summary>
        /// <param name="item">The item to select.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/>.</exception>
        public void SetSelection(SolutionItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            SetSelection(new[] { item });
        }

        /// <summary>
        /// Sets the selected items in the Solution Explorer window.
        /// </summary>
        /// <param name="items">The items to select.</param>
        public void SetSelection(IEnumerable<SolutionItem> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            bool addToSelection = false;

            foreach (SolutionItem item in items)
            {
                item.GetItemInfo(out IVsHierarchy hierarchy, out uint itemID, out IVsHierarchyItem _);
                if (hierarchy is IVsUIHierarchy uiHierarchy)
                {
                    if (!addToSelection)
                    {
                        // Try to select the item. This will remove the selection from all other items.
                        UIHierarchyWindow.ExpandItem(uiHierarchy, itemID, EXPANDFLAGS.EXPF_SelectItem);

                        // Check if the item was actually selected. The item may not be visible
                        // or even exist, and `ExpandItem` doesn't fail in that scenario, so the
                        // only way to check if the item was selected is to get its current state.
                        if (ErrorHandler.Succeeded(UIHierarchyWindow.GetItemState(uiHierarchy, itemID, (uint)__VSHIERARCHYITEMSTATE.HIS_Selected, out uint state)))
                        {
                            if (state == (uint)__VSHIERARCHYITEMSTATE.HIS_Selected)
                            {
                                // The item has been selected, which means that for the
                                // next item that we need to select, we need to _add_
                                // to the selection instead of replace the selection.
                                addToSelection = true;
                            }
                        }
                    }
                    else
                    {
                        UIHierarchyWindow.ExpandItem(uiHierarchy, itemID, EXPANDFLAGS.EXPF_AddSelectItem);
                    }
                }
            }
        }

        /// <summary>
        /// Begins editing of the specified item.
        /// </summary>
        /// <param name="item">The item to being editing the label of.</param>
        public void EditLabel(SolutionItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            item.GetItemInfo(out IVsHierarchy hierarchy, out uint itemID, out IVsHierarchyItem _);
            if (hierarchy is IVsUIHierarchy uiHierarchy)
            {
                UIHierarchyWindow.ExpandItem(uiHierarchy, itemID, EXPANDFLAGS.EXPF_EditItemLabel);
            }
        }

        /// <summary>
        /// Expands the specified item.
        /// </summary>
        /// <param name="item">The item to expand.</param>
        /// <param name="mode">Specifies how the item will be expanded.</param>
        public void Expand(SolutionItem item, SolutionItemExpansionMode mode = SolutionItemExpansionMode.Single)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            Expand(new[] { item }, mode);
        }

        /// <summary>
        /// Expands the specified items.
        /// </summary>
        /// <param name="items">The items to expand.</param>
        /// <param name="mode">Specifies how the items will be expanded.</param>
        public void Expand(IEnumerable<SolutionItem> items, SolutionItemExpansionMode mode = SolutionItemExpansionMode.Single)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            // Although the `EXPANDFLAGS` has the `[Flags]` attribute, the values 
            // cannot actually be combined because the values are sequential.
            // We need to make multiple calls for each mode that is set.
            if ((mode & SolutionItemExpansionMode.Ancestors) == SolutionItemExpansionMode.Ancestors)
            {
                Expand(items, EXPANDFLAGS.EXPF_ExpandParentsToShowItem);
            }

            // Expanding recursively will also expand the given items, so if we
            // expand recurisvely, then we don't need to check for the `Single` mode.
            if ((mode & SolutionItemExpansionMode.Recursive) == SolutionItemExpansionMode.Recursive)
            {
                Expand(items, EXPANDFLAGS.EXPF_ExpandFolderRecursively);
            }
            else if ((mode & SolutionItemExpansionMode.Single) == SolutionItemExpansionMode.Single)
            {
                Expand(items, EXPANDFLAGS.EXPF_ExpandFolder);
            }
        }

        private void Expand(IEnumerable<SolutionItem> items, EXPANDFLAGS flag)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (SolutionItem item in items)
            {
                item.GetItemInfo(out IVsHierarchy hierarchy, out uint itemID, out IVsHierarchyItem _);
                if (hierarchy is IVsUIHierarchy uiHierarchy)
                {
                    UIHierarchyWindow.ExpandItem(uiHierarchy, itemID, flag);
                }
            }
        }

        /// <summary>
        /// Collapses the specified item.
        /// </summary>
        /// <param name="item">The item to collapse.</param>
        public void Collapse(SolutionItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            Collapse(new[] { item });
        }

        /// <summary>
        /// Collapses the specified items.
        /// </summary>
        /// <param name="items">The items to collapse.</param>
        public void Collapse(IEnumerable<SolutionItem> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (SolutionItem item in items)
            {
                item.GetItemInfo(out IVsHierarchy hierarchy, out uint itemID, out IVsHierarchyItem _);
                if (hierarchy is IVsUIHierarchy uiHierarchy)
                {
                    UIHierarchyWindow.ExpandItem(uiHierarchy, itemID, EXPANDFLAGS.EXPF_CollapseFolder);
                }
            }
        }
    }

    /// <summary>
    /// Defines how a Solution Explorer item should be expanded.
    /// </summary>
    [Flags]
    public enum SolutionItemExpansionMode
    {
        /// <summary>
        /// The item and its ancestors and descendants will not be expanded.
        /// </summary>
        None = 0,
        /// <summary>
        /// Only the specified item will be expanded. Ancestors and descendants will not be expanded.
        /// </summary>
        Single = 1,
        /// <summary>
        /// The specified item and all descendants will be expanded. Ancestors will not be expanded.
        /// </summary>
        Recursive = 2,
        /// <summary>
        /// The ancestors of the item will be expanded. The item itself and its children will not be expanded.
        /// </summary>
        Ancestors = 4
    }
}
