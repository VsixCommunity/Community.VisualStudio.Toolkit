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
    /// Services related to the selection of windows and items in solution.
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
            List<SolutionItem> items = new();

            foreach (IVsHierarchyItem hierarchy in hierarchies)
            {
                SolutionItem? item = await SolutionItem.FromHierarchyAsync(hierarchy.HierarchyIdentity.Hierarchy, hierarchy.HierarchyIdentity.ItemID);

                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
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
        /// Sets the current UI context.
        /// </summary>
        /// <param name="uiContextGuid">The GUID to uniquely identify the UI context.</param>
        /// <param name="isActive">Determines if the UI context is active or not.</param>
        public Task SetUIContextAsync(string uiContextGuid, bool isActive) 
            => SetUIContextAsync(new Guid(uiContextGuid), isActive);

        /// <summary>
        /// Sets the current UI context.
        /// </summary>
        /// <param name="uiContextGuid">The GUID to uniquely identify the UI context.</param>
        /// <param name="isActive">Determines if the UI context is active or not.</param>
        public async Task SetUIContextAsync(Guid uiContextGuid, bool isActive)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection svc = await VS.Services.GetMonitorSelectionAsync();

            int cookieResult = svc.GetCmdUIContextCookie(uiContextGuid, out uint cookie);
            ErrorHandler.ThrowOnFailure(cookieResult);

            int setContextResult = svc.SetCmdUIContext(cookie, isActive ? 1 : 0);
            ErrorHandler.ThrowOnFailure(setContextResult);
        }

        /// <summary>
        /// Gets the currently selected hierarchy items.
        /// </summary>
        internal async Task<IEnumerable<IVsHierarchyItem>> GetSelectedHierarchiesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsMonitorSelection svc = await VS.Services.GetMonitorSelectionAsync();
            IntPtr hierPtr = IntPtr.Zero;
            IntPtr containerPtr = IntPtr.Zero;

            List<IVsHierarchyItem> results = new();

            try
            {
                svc.GetCurrentSelection(out hierPtr, out uint itemId, out IVsMultiItemSelect multiSelect, out containerPtr);

                if (itemId == VSConstants.VSITEMID_SELECTION)
                {
                    multiSelect.GetSelectionInfo(out uint itemCount, out int fSingleHierarchy);

                    VSITEMSELECTION[] items = new VSITEMSELECTION[itemCount];
                    multiSelect.GetSelectedItems(0, itemCount, items);

                    results.Capacity = (int)itemCount;

                    foreach (VSITEMSELECTION item in items)
                    {
                        IVsHierarchyItem? hierItem = await item.pHier.ToHierarchyItemAsync(item.itemid);

                        if (hierItem != null)
                        {
                            results.Add(hierItem);
                        }
                        else
                        {
                            IVsHierarchy solution = (IVsHierarchy)await VS.Services.GetSolutionAsync();
                            IVsHierarchyItem? sol = await solution.ToHierarchyItemAsync(VSConstants.VSITEMID_ROOT);

                            if (sol != null)
                            {
                                results.Add(sol);
                            }
                        }
                    }
                }
                else if (itemId == VSConstants.VSITEMID_NIL)
                {
                    // Empty Solution Explorer or nothing selected, so don't add anything.
                }
                else if (hierPtr != IntPtr.Zero)
                {
                    IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetUniqueObjectForIUnknown(hierPtr);
                    IVsHierarchyItem? hierItem = await hierarchy.ToHierarchyItemAsync(itemId);

                    if (hierItem != null)
                    {
                        results.Add(hierItem);
                    }
                }
                else if (await VS.Services.GetSolutionAsync() is IVsHierarchy solution)
                {
                    IVsHierarchyItem? sol = await solution.ToHierarchyItemAsync(VSConstants.VSITEMID_ROOT);

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
