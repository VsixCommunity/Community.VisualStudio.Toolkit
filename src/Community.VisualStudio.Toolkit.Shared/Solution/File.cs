using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents a physical file in the solution hierarchy.
    /// </summary>
    public class File : SolutionItem
    {
        internal File(IVsHierarchyItem item, SolutionItemType type) : base(item, type)
        { ThreadHelper.ThrowIfNotOnUIThread(); }

        /// <summary>
        /// The containing folder of the file.
        /// </summary>
        public string Folder => Path.GetDirectoryName(FullPath);

        /// <summary>
        /// The file extension starting with a dot.
        /// </summary>
        public string Extension => Path.GetExtension(FullPath);

        /// <summary>
        /// The project containing this file, or <see langword="null"/>.
        /// </summary>
        public Project? ContainingProject => FindParent(SolutionItemType.Project) as Project;

        /// <summary>
        /// Opens the item in the editor window.
        /// </summary>
        /// <returns><see langword="null"/> if the item was not succesfully opened.</returns>
        public async Task<WindowFrame?> OpenAsync()
        {
            if (!string.IsNullOrEmpty(FullPath))
            {
                await VS.Documents.OpenViaProjectAsync(FullPath!);
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
                GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out _);

                if (hierarchy is IVsProject2 project)
                {
                    project.RemoveItem(0, itemId, out int result);
                    return result == 1;
                }
            }

            return false;
        }

        /// <summary>
        /// Nests a file under this file by setting its <c>DependentUpon</c> property..
        /// </summary>
        public Task AddNestedFileAsync(File fileToNest)
            => fileToNest.TrySetAttributeAsync("DependentUpon", Name);

        /// <summary>
        /// Tries to set an attribute in the project file for the item.
        /// </summary>
        public async Task<bool> TrySetAttributeAsync(string name, string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out _);

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
            GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out _);

            if (hierarchy is IVsBuildPropertyStorage storage)
            {
                storage.GetItemAttribute(itemId, name, out string value);
                return value;
            }

            return null;
        }

        /// <summary>
        /// Finds the item in the solution matching the specified file path.
        /// </summary>
        /// <param name="filePath">The absolute file path of a file that exists in the solution.</param>
        /// <returns><see langword="null"/> if the file wasn't found in the solution.</returns>
        public static async Task<File?> FromFileAsync(string filePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IEnumerable<IVsHierarchy> projects = await VS.Solutions.GetAllProjectHierarchiesAsync();

            VSDOCUMENTPRIORITY[] priority = new VSDOCUMENTPRIORITY[1];

            foreach (IVsHierarchy hierarchy in projects)
            {
                IVsProject proj = (IVsProject)hierarchy;
                proj.IsDocumentInProject(filePath, out int isFound, priority, out uint itemId);

                if (isFound == 1)
                {
                    return await FromHierarchyAsync(hierarchy, itemId) as File;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the item in the solution matching the specified file path.
        /// </summary>
        /// <param name="filePaths">The absolute file paths of files that exist in the solution.</param>
        public static async Task<IEnumerable<File>> FromFilesAsync(params string[] filePaths)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            List<File> items = new();

            foreach (string filePath in filePaths)
            {
                File? item = await FromFileAsync(filePath);

                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }
    }
}
