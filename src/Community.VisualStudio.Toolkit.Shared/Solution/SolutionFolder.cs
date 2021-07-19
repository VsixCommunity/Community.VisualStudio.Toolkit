using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents a solution folder in the solution
    /// </summary>
    public class SolutionFolder : SolutionItem
    {
        internal SolutionFolder(IVsHierarchyItem item, SolutionItemType type) : base(item, type)
        { ThreadHelper.ThrowIfNotOnUIThread(); }

        /// <summary>
        /// Adds one or more files to the solution folder.
        /// </summary>
        public async Task<IEnumerable<File>> AddExistingFilesAsync(params string[] files)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsUIShell uiShell = await VS.Services.GetUIShellAsync();
            uiShell.GetDialogOwnerHwnd(out IntPtr hwndDlgOwner);

            GetItemInfo(out IVsHierarchy hierarchy, out _, out _);

            Guid rguidEditorType = Guid.Empty, rguidLogicalView = Guid.Empty;
            VSADDRESULT[] result = new VSADDRESULT[1];
            IVsProject3 project3 = (IVsProject3)hierarchy;

            project3.AddItemWithSpecific(itemidLoc: (uint)VSConstants.VSITEMID.Root,
                dwAddItemOperation: VSADDITEMOPERATION.VSADDITEMOP_OPENFILE,
                pszItemName: "test",
                cFilesToOpen: (uint)files.Count(), //The name of the parameter is misleading, it's the number of files to process, 
                                                   //and whether to open in editor or not is determined by other flag
                rgpszFilesToOpen: files,
                hwndDlgOwner: hwndDlgOwner,
                grfEditorFlags: 0u, //We do not want to open in the editor
                rguidEditorType: ref rguidEditorType,
                pszPhysicalView: null,
                rguidLogicalView: ref rguidLogicalView,
                pResult: result);

            return await File.FromFilesAsync(files);
        }

        /// <summary>
        /// Tries to remove the solution folder from the solution.
        /// </summary>
        public async Task<bool> TryRemoveAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SolutionItem? solution = FindParent(SolutionItemType.Solution);

            if (solution != null)
            {
                GetItemInfo(out IVsHierarchy hierarchy, out _, out _);

                if (hierarchy is IVsSolution ivsSolution)
                {
                    int hr = ivsSolution.CloseSolutionElement(0, hierarchy, 0);
                    return hr == VSConstants.S_OK;
                }
            }

            return false;
        }
    }
}
