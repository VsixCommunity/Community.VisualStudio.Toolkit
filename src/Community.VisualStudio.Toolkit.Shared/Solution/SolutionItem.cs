using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// An item reprensenting a file, folder, project or other item in Solution Explorer.
    /// </summary>
    [DebuggerDisplay("{Name} ({Type})")]
    public class SolutionItem
    {
        private SolutionItem? _parent;
        private IEnumerable<SolutionItem?>? _children;
        private readonly IVsHierarchyItem _item;
        private readonly IVsHierarchy _hierarchy;
        private readonly uint _itemId;

        private SolutionItem(IVsHierarchyItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _item = item;

            _hierarchy = item.HierarchyIdentity.IsNestedItem ? item.HierarchyIdentity.NestedHierarchy : item.HierarchyIdentity.Hierarchy;
            _itemId = item.HierarchyIdentity.IsNestedItem ? item.HierarchyIdentity.NestedItemID : item.HierarchyIdentity.ItemID;

            Name = item.Text;
            Type = GetNodeType(item.HierarchyIdentity);
            FileName = GetFileName();
        }

        /// <summary>
        /// The display name of the node
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The absolute file path on disk.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// The type of node.
        /// </summary>
        public NodeType Type { get; }

        /// <summary>
        /// The parent node. Is <see langword="null"/> when there is no parent.
        /// </summary>
        public SolutionItem? Parent => _parent ??= FromHierarchyItem(_item.Parent);

        /// <summary>
        /// A list of child nodes.
        /// </summary>
        public IEnumerable<SolutionItem?> Children => _children ??= _item.Children.Select(t => FromHierarchyItem(t));

        /// <summary>
        /// Get information from the underlying data types.
        /// </summary>
        public void GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out IVsHierarchyItem hierarchyItem)
        {
            hierarchy = _hierarchy;
            itemId = _itemId;
            hierarchyItem = _item;
        }

        /// <summary>
        /// Checks what kind the project is.
        /// </summary>
        /// <param name="typeGuid">Use the <see cref="ProjectTypes"/> collection for known guids.</param>
        public bool IsKind(string typeGuid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Type == NodeType.Project || Type == NodeType.VirtualProject)
            {
                return _hierarchy.IsProjectOfType(typeGuid);
            }

            return false;
        }

        /// <summary>
        /// Adds a file as a child to the item. 
        /// </summary>
        /// <param name="files">A list of absolute file paths.</param>
        public async Task<IEnumerable<SolutionItem>?> AddItemsAsync(params string[] files)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            List<SolutionItem>? items = new();

            // Add solution folder
            if (Type == NodeType.Solution)
            {
                var guid = new Guid(ProjectTypes.SOLUTION_FOLDER_OTHER);
                Guid iidProject = typeof(IVsHierarchy).GUID;
                IVsSolution sol = await VS.Services.GetSolutionServiceAsync();

                foreach (var file in files)
                {
                    var solFldName = Path.GetDirectoryName(file);
                    var hr = sol.CreateProject(ref guid, null, null, solFldName, 0, ref iidProject, out IntPtr ptr);

                    if (hr == VSConstants.S_OK && ptr != IntPtr.Zero)
                    {
                        if (Marshal.GetObjectForIUnknown(ptr) is IVsHierarchy hier)
                        {
                            if (await FromHierarchyAsync(hier, (uint)VSConstants.VSITEMID.Root) is SolutionItem item)
                            {
                                items.Add(item);
                            }
                        }

                        Marshal.Release(ptr);
                    }
                }
            }
            // Add file
            else if (Type == NodeType.Project || Type == NodeType.PhysicalFolder || Type == NodeType.PhysicalFile)
            {
                var result = new VSADDRESULT[files.Count()];
                var ip = (IVsProject4)_hierarchy;

                ip.AddItem(_itemId, VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE, string.Empty, (uint)files.Count(), files, IntPtr.Zero, result);

                foreach (var file in files)
                {
                    SolutionItem? item = await FromFileAsync(file);

                    if (item != null)
                    {
                        items.Add(item);

                        if (Type == NodeType.PhysicalFile)
                        {
                            item.TrySetAttribute("DependentUpon", Name);
                        }
                    }
                }
            }
            else if (Type == NodeType.SolutionFolder)
            {
                // TODO: Find a way to do this without using the DTE.
                EnvDTE.Project? project = HierarchyUtilities.GetProject(_item);

                foreach (var file in files)
                {
                    (project?.Object as EnvDTE80.SolutionFolder)?.AddFromFile(file);
                }
            }

            return items;
        }

        /// <summary>
        /// Tries to set an attribute in the project file for the item.
        /// </summary>
        public bool TrySetAttribute(string name, string value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_hierarchy is IVsBuildPropertyStorage storage)
            {
                if (Type == NodeType.Project || Type == NodeType.VirtualProject || Type == NodeType.MiscProject)
                {
                    storage.SetPropertyValue(name, "", (uint)_PersistStorageType.PST_PROJECT_FILE, value);
                    return true;
                }
                else if (Type == NodeType.PhysicalFile || Type == NodeType.PhysicalFolder)
                {
                    storage.SetItemAttribute(_itemId, name, value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to retrieve an attribute value from the project file for the item.
        /// </summary>
        public bool TryGetAttribute(string name, out string? value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            value = null;

            if (_hierarchy is IVsBuildPropertyStorage storage)
            {
                if (Type == NodeType.Project || Type == NodeType.VirtualProject || Type == NodeType.MiscProject)
                {
                    storage.GetPropertyValue(name, "", (uint)_PersistStorageType.PST_PROJECT_FILE, out value);
                    return true;
                }
                else if (Type == NodeType.PhysicalFile || Type == NodeType.PhysicalFolder)
                {
                    storage.GetItemAttribute(_itemId, name, out value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Find the nearest parent matching the specified type.
        /// </summary>
        public SolutionItem? FindParent(NodeType type)
        {
            SolutionItem? parent = Parent;

            while (parent != null)
            {
                if (parent.Type == type)
                {
                    return parent;
                }

                parent = parent.Parent;
            }

            return null;
        }

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
        /// Tries to remove the solution item from the solution.
        /// </summary>
        public async Task<bool> TryRemoveAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            SolutionItem? parent = FindParent(NodeType.Project) ?? FindParent(NodeType.SolutionFolder) ?? FindParent(NodeType.Solution);

            if (parent == null)
            {
                return false;
            }

            if (Type == NodeType.PhysicalFile)
            {
                if (parent._hierarchy is IVsProject2 project)
                {
                    project.RemoveItem(0, _itemId, out var result);
                    return result == 1;
                }
            }
            else
            {
                // TODO: Figure out how to remove projects and solution folders without the DTE
                EnvDTE80.DTE2? dte = await VS.GetRequiredServiceAsync<EnvDTE.DTE, EnvDTE80.DTE2>();
                EnvDTE.Project? project = HierarchyUtilities.GetProject(_item);
                dte.Solution?.Remove(project);
            }

            return false;
        }

        /// <summary>
        /// Creates a new instance based on a hierarchy.
        /// </summary>
        public static async Task<SolutionItem?> FromHierarchyAsync(IVsHierarchy hierarchy, uint itemId)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsHierarchyItem? item = await hierarchy.ToHierarcyItemAsync(itemId);

            return FromHierarchyItem(item);
        }

        /// <summary>
        /// Creates a new instance based on a hierarchy item.
        /// </summary>
        public static SolutionItem? FromHierarchyItem(IVsHierarchyItem? item)
        {
            if (item == null)
            {
                return null;
            }

            return new SolutionItem(item);
        }

        /// <summary>
        /// Finds the item in the solution matching the specified file path.
        /// </summary>
        /// <param name="filePath">The absolute file path of a file that exist in the solution.</param>
        /// <returns><see langword="null"/> if the file wasn't found in the solution.</returns>
        public static async Task<SolutionItem?> FromFileAsync(string filePath)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IEnumerable<IVsHierarchy>? projects = await VS.Solution.GetAllProjectHierarchiesAsync();

            foreach (IVsHierarchy? hierarchy in projects)
            {
                var proj = (IVsProject5)hierarchy;
                proj.IsDocumentInProject2(filePath, out var isFound, out _, out var itemId);

                if (isFound == 1)
                {
                    return await FromHierarchyAsync(hierarchy, itemId);
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the item in the solution matching the specified file path.
        /// </summary>
        /// <param name="filePaths">The absolute file paths of files that exist in the solution.</param>
        public static async Task<IEnumerable<SolutionItem>?> FromFilesAsync(params string[] filePaths)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            List<SolutionItem> items = new();

            foreach (var filePath in filePaths)
            {
                SolutionItem? item = await FromFileAsync(filePath);

                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private NodeType GetNodeType(IVsHierarchyItemIdentity identity)
        {
            if (HierarchyUtilities.IsSolutionNode(identity))
            {
                return NodeType.Solution;
            }
            else if (HierarchyUtilities.IsSolutionFolder(identity))
            {
                return NodeType.SolutionFolder;
            }
            else if (HierarchyUtilities.IsMiscellaneousProject(identity))
            {
                return NodeType.MiscProject;
            }
            else if (HierarchyUtilities.IsVirtualProject(identity))
            {
                return NodeType.VirtualProject;
            }
            else if (HierarchyUtilities.IsProject(identity))
            {
                return NodeType.Project;
            }
            else if (HierarchyUtilities.IsPhysicalFile(identity))
            {
                return NodeType.PhysicalFile;
            }
            else if (HierarchyUtilities.IsPhysicalFolder(identity))
            {
                return NodeType.PhysicalFolder;
            }

            return NodeType.Unknown;
        }

        private string? GetFileName()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Type == NodeType.SolutionFolder)
            {
                return null;
            }

            _hierarchy.GetCanonicalName(_itemId, out var fileName);

            if (_hierarchy is IVsProject project && project.GetMkDocument(_itemId, out fileName) == VSConstants.S_OK)
            {
                return fileName;
            }

            if (_hierarchy is IVsSolution solution && solution.GetSolutionInfo(out _, out var slnFile, out _) == VSConstants.S_OK)
            {
                return slnFile;
            }

            return fileName;
        }
    }

    /// <summary>
    /// Types of nodes.
    /// </summary>
    public enum NodeType
    {
        /// <summary>Physical file on disk</summary>
        PhysicalFile,
        /// <summary>Physical folder on disk</summary>
        PhysicalFolder,
        /// <summary>A project</summary>
        Project,
        /// <summary>A miscellaneous project</summary>
        MiscProject,
        /// <summary>A virtual project</summary>
        VirtualProject,
        /// <summary>The solution</summary>
        Solution,
        /// <summary>A solution folder</summary>
        SolutionFolder,
        /// <summary>Unknown node type</summary>
        Unknown,
    }
}
