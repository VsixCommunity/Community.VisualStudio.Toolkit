using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

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
        private readonly IVsHierarchy _hierarchy;
        private readonly uint _itemId;

        private SolutionItem(IVsHierarchyItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _hierarchy = item.HierarchyIdentity.IsNestedItem ? item.HierarchyIdentity.NestedHierarchy : item.HierarchyIdentity.Hierarchy;
            _itemId = item.HierarchyIdentity.IsNestedItem ? item.HierarchyIdentity.NestedItemID : item.HierarchyIdentity.ItemID;

            Item = item;
            Name = item.Text;
            Type = GetNodeType(item.HierarchyIdentity);
            IsFaulted = HierarchyUtilities.IsFaultedProject(item.HierarchyIdentity);
            IsHidden = HierarchyUtilities.IsHiddenItem(_hierarchy, _itemId);
            FileName = GetFileName();
        }

        /// <summary>
        /// The underlying hierarchy item.
        /// </summary>
        public IVsHierarchyItem Item { get; }

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
        /// A value indicating if the node is in a faulted state.
        /// </summary>
        public bool IsFaulted { get; }

        /// <summary>
        /// A value indicating if the node is hidden.
        /// </summary>
        public bool IsHidden { get; }

        /// <summary>
        /// The parent node. Is <see langword="null"/> when there is no parent.
        /// </summary>
        public SolutionItem? Parent => _parent ??= FromHierarchyItem(Item.Parent);

        /// <summary>
        /// A list of child nodes.
        /// </summary>
        public IEnumerable<SolutionItem?> Children => _children ??= Item.Children.Select(t => FromHierarchyItem(t));

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
        public async Task AddFilesAsync(params string[] files)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (Type == NodeType.Project || Type == NodeType.PhysicalFolder)
            {
                var result = new VSADDRESULT[files.Count()];
                var ip = (IVsProject)_hierarchy;

                ip.AddItem(_itemId,
                           VSADDITEMOPERATION.VSADDITEMOP_LINKTOFILE,
                           string.Empty,
                           (uint)files.Count(),
                           files,
                           IntPtr.Zero,
                           result);
            }
            else if (Type == NodeType.SolutionFolder)
            {
                //TODO: Implement
            }
            else if (Type == NodeType.PhysicalFile)
            {
                // TODO: Implement support for nesting files by setting <DependentUpon>
            }
        }

        /// <summary>
        ///  Adds a solution folder
        /// </summary>
        public async Task<SolutionItem?> AddFolderAsync(string folderName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (Type == NodeType.Solution)
            {
                var solutionFolderGuid = new Guid(ProjectTypes.SOLUTION_FOLDER_OTHER);
                Guid iidProject = typeof(IVsHierarchy).GUID;
                IVsSolution sol = await VS.Solution.GetSolutionAsync();

                var hr = sol.CreateProject(
                    ref solutionFolderGuid,
                    null,
                    null,
                    folderName,
                    0,
                    ref iidProject,
                    out IntPtr ptr);

                if (hr == VSConstants.S_OK && ptr != IntPtr.Zero)
                {
                    var hier = (IVsHierarchy)Marshal.GetObjectForIUnknown(ptr);

                    if (hier != null)
                    {
                        return await FromHierarchyAsync(hier, (uint)VSConstants.VSITEMID.Root);
                    }
                }
            }
            else if (Type == NodeType.SolutionFolder)
            {
                // TODO: Implement
            }
            else if (Type == NodeType.PhysicalFolder)
            {
                // TODO: Implement
            }

            return null;
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
        /// <returns><see langword="true"/> if the item was succesfully opened; otherwise <see langword="false"/>.</returns>
        public async Task<bool> TryOpenAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var hr = VSConstants.S_FALSE;
            var ip = (IVsProject)_hierarchy;

            if (ip != null)
            {
                hr = ip.OpenItem(_itemId, Guid.Empty, IntPtr.Zero, out _);
            }

            return hr == VSConstants.S_OK;
        }

        /// <summary>
        /// Converts the node a <see cref="EnvDTE.Project"/>.
        /// </summary>
        public EnvDTE.Project? ToProject()
        {
            return HierarchyUtilities.GetProject(Item);
        }

        /// <summary>
        /// Converts the node a <see cref="EnvDTE.ProjectItem"/>.
        /// </summary>
        public EnvDTE.ProjectItem? ToProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_hierarchy.TryGetItemProperty(_itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out object? obj))
            {
                return obj as EnvDTE.ProjectItem;
            }

            return null;
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
                var proj = (IVsProject)hierarchy;
                var priority = new VSDOCUMENTPRIORITY[1];
                proj.IsDocumentInProject(filePath, out var isFound, priority, out var itemId);

                if (isFound == 1)
                {
                    return await FromHierarchyAsync(hierarchy, itemId);
                }
            }

            return null;
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

            if (_hierarchy is IVsSolution solution && solution.GetFilePath() is string slnFile)
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
