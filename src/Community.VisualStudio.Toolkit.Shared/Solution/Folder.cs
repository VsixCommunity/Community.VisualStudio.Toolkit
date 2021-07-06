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
    /// Represents a physical file in the solution hierarchy.
    /// </summary>
    public class Folder : SolutionItem
    {
        internal Folder(IVsHierarchyItem item) : base(item)
        { ThreadHelper.ThrowIfNotOnUIThread(); }

        /// <summary>
        /// Opens the item in the editor window.
        /// </summary>
        /// <returns><see langword="null"/> if the item was not succesfully opened.</returns>
        public async Task<IEnumerable<File>> AddExistingFilesAsync(params string[] files)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            List<File>? items = new();
            GetItemInfo(out IVsHierarchy? hierarchy, out uint itemId, out _);

            VSADDRESULT[] result = new VSADDRESULT[files.Count()];
            IVsProject ip = (IVsProject)hierarchy;

            ErrorHandler.ThrowOnFailure(ip.AddItem(itemId, VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE, string.Empty, (uint)files.Count(), files, IntPtr.Zero, result));

            foreach (string file in files)
            {
                File? item = await File.FromFileAsync(file);

                if (item != null)
                {
                    items.Add(item);
                    await item.TrySetAttributeAsync("DependentUpon", Name);
                }
            }

            return items;
        }
    }
}
