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
    /// Represents a physical folder in the solution hierarchy.
    /// </summary>
    public class Folder : SolutionItem
    {
        internal Folder(IVsHierarchyItem item, SolutionItemType type) : base(item, type)
        { ThreadHelper.ThrowIfNotOnUIThread(); }

        /// <summary>
        /// The project containing this folder, or <see langword="null"/>.
        /// </summary>
        public Project? ContainingProject => FindParent(SolutionItemType.Project) as Project;

        /// <summary>
        /// Add existing files to the folder.
        /// </summary>
        /// <returns>A list of <see cref="File"/> items added to the folder.</returns>
        public async Task<IEnumerable<File>> AddExistingFilesAsync(params string[] filePaths)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out _);

            VSADDRESULT[] result = new VSADDRESULT[filePaths.Count()];
            IVsProject ip = (IVsProject)hierarchy;

            ErrorHandler.ThrowOnFailure(ip.AddItem(itemId, VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE, string.Empty, (uint)filePaths.Count(), filePaths, IntPtr.Zero, result));

            List<File> files = new();

            foreach (string filePath in filePaths)
            {
                File? file = await File.FromFileAsync(filePath);

                if (file != null)
                {
                    files.Add(file);                    
                }
            }

            return files;
        }
    }
}
