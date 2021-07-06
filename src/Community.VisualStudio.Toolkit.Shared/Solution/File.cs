using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents a physical file in the solution hierarchy.
    /// </summary>
    public class File : SolutionItem
    {
        internal File(IVsHierarchyItem item) : base(item)
        { ThreadHelper.ThrowIfNotOnUIThread(); }

        /// <summary>
        /// Opens the item in the editor window.
        /// </summary>
        /// <returns><see langword="null"/> if the item was not succesfully opened.</returns>
        public async Task<WindowFrame?> OpenAsync()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                await VS.Documents.OpenViaProjectAsync(FileName!);
            }

            return null;
        }

        /// <summary>
        /// Tries to remove the file from the project or solution folder.
        /// </summary>
        public async Task<bool> TryRemoveAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SolutionItem? parent = FindParent(SolutionItemType.Project) ?? FindParent(SolutionItemType.SolutionFolder);

            if (parent != null)
            {
                GetItemInfo(out IVsHierarchy? hierarchy, out uint itemId, out _);

                if (hierarchy is IVsProject2 project)
                {
                    project.RemoveItem(0, itemId, out int result);
                    return result == 1;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to set an attribute in the project file for the item.
        /// </summary>
        public async Task<bool> TrySetAttributeAsync(string name, string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            GetItemInfo(out IVsHierarchy? hierarchy, out uint itemId, out _);

            if (hierarchy is IVsBuildPropertyStorage storage)
            {
                storage.SetItemAttribute(itemId, name, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to retrieve an attribute value from the project file for the item.
        /// </summary>
        /// <returns><see langword="null"/> if the attribute doesn't exist.</returns>
        public async Task<string?> GetAttributeAsync(string name)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            GetItemInfo(out IVsHierarchy? hierarchy, out uint itemId, out _);

            if (hierarchy is IVsBuildPropertyStorage storage)
            {
                storage.GetItemAttribute(itemId, name, out string? value);
                return value;
            }

            return null;
        }
    }
}
