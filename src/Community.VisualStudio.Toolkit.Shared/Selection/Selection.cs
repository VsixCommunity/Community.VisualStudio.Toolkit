using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Services related to the selection of windows and nodes.
    /// </summary>
    public class Selection
    {
        internal Selection()
        { }

        /// <summary>
        /// Gets the currently selected items.
        /// </summary>
        public async Task<IEnumerable<SolutionItem>> GetSelectedItemsAsync()
        {
            IEnumerable<IVsHierarchyItem>? hierarchies = await GetSelectedHierarchiesAsync();
            List<SolutionItem> nodes = new();

            foreach (IVsHierarchyItem hierarchy in hierarchies)
            {
                SolutionItem? node = await SolutionItem.FromHierarchyAsync(hierarchy.HierarchyIdentity.Hierarchy, hierarchy.HierarchyIdentity.ItemID);

                if (node != null)
                {
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        /// <summary>
        /// Gets the currently selected item. If more than one item is selected, it returns the first one.
        /// </summary>
        /// <remarks><see langword="null"/> if no items are selected.</remarks>
        public async Task<SolutionItem?> GetSelectedItemAsync()
        {
            IEnumerable<SolutionItem>? items = await GetSelectedItemsAsync();
            return items?.FirstOrDefault();
        }

        /// <summary>
        /// Sets the current UI Context.
        /// </summary>
        /// <param name="uiContextGuid">The guid to uniquely identify the UI context.</param>
        /// <param name="isActive">Determines if the UI Context is active or not.</param>
        public Task SetUIContextAsync(string uiContextGuid, bool isActive) 
            => SetUIContextAsync(new Guid(uiContextGuid), isActive);

        /// <summary>
        /// Sets the current UI Context.
        /// </summary>
        /// <param name="uiContextGuid">The guid to uniquely identify the UI context.</param>
        /// <param name="isActive">Determines if the UI Context is active or not.</param>
        public async Task SetUIContextAsync(Guid uiContextGuid, bool isActive)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection? svc = await VS.Services.GetMonitorSelectionAsync();

            var cookieResult = svc.GetCmdUIContextCookie(uiContextGuid, out var cookie);
            ErrorHandler.ThrowOnFailure(cookieResult);

            var setContextResult = svc.SetCmdUIContext(cookie, isActive ? 1 : 0);
            ErrorHandler.ThrowOnFailure(setContextResult);
        }

        /// <summary>
        /// Gets the currently selected hierarchy items.
        /// </summary>
        internal async Task<IEnumerable<IVsHierarchyItem>> GetSelectedHierarchiesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection? svc = await VS.Services.GetMonitorSelectionAsync();
            IntPtr hierPtr = IntPtr.Zero;
            IntPtr containerPtr = IntPtr.Zero;

            List<IVsHierarchyItem> results = new();

            try
            {
                svc.GetCurrentSelection(out hierPtr, out var itemId, out IVsMultiItemSelect multiSelect, out containerPtr);

                if (itemId == VSConstants.VSITEMID_SELECTION)
                {
                    multiSelect.GetSelectionInfo(out var itemCount, out var fSingleHierarchy);

                    var items = new VSITEMSELECTION[itemCount];
                    multiSelect.GetSelectedItems(0, itemCount, items);

                    foreach (VSITEMSELECTION item in items)
                    {
                        IVsHierarchyItem? hierItem = await item.pHier.ToHierarcyItemAsync(item.itemid);
                        if (hierItem != null && !results.Contains(hierItem))
                        {
                            results.Add(hierItem);
                        }
                    }
                }
                else if (hierPtr != IntPtr.Zero)
                {
                    var hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierPtr);
                    IVsHierarchyItem? hierItem = await hierarchy.ToHierarcyItemAsync(itemId);

                    if (hierItem != null)
                    {
                        results.Add(hierItem);
                    }
                }
                else if (await VS.Services.GetSolutionAsync() is IVsHierarchy solution)
                {
                    IVsHierarchyItem? sol = await solution.ToHierarcyItemAsync(VSConstants.VSITEMID_ROOT);
                    if (sol != null)
                    {
                        results.Add(sol);
                    }
                }
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
    }
}
