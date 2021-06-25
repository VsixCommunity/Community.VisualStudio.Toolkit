using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A node reprensenting a file, folder, project or other item in Solution Explorer.
    /// </summary>
    [DebuggerDisplay("{Name} ({Type})")]
    public class ItemNode
    {
        private ItemNode? _parent;
        private IEnumerable<ItemNode?>? _children;
        private IVsHierarchy _hierarchy;
        private uint _itemId;

        private ItemNode(IVsHierarchyItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _hierarchy = item.HierarchyIdentity.IsNestedItem ? item.HierarchyIdentity.NestedHierarchy : item.HierarchyIdentity.Hierarchy;
            _itemId = item.HierarchyIdentity.IsNestedItem ? item.HierarchyIdentity.NestedItemID : item.HierarchyIdentity.ItemID;

            _hierarchy.GetCanonicalName(_itemId, out var fileName);

            FileName = fileName;
            Item = item;
            Name = item.Text;
            Type = GetNodeType(item.HierarchyIdentity);
            IsExpandable = HierarchyUtilities.IsExpandable(item.HierarchyIdentity);
            IsFaulted = HierarchyUtilities.IsFaultedProject(item.HierarchyIdentity);
            IsHidden = HierarchyUtilities.IsHiddenItem(_hierarchy, _itemId);
            IsRoot = item.HierarchyIdentity.IsRoot;
            IsNested = item.HierarchyIdentity.IsNestedItem;
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
        public string FileName { get; set; }

        /// <summary>
        /// The type of node.
        /// </summary>
        public NodeType Type { get; }

        /// <summary>
        /// A value indicating if the node can be expanded.
        /// </summary>
        public bool IsExpandable { get; }

        /// <summary>
        /// A value indicating if the node is in a faulted state.
        /// </summary>
        public bool IsFaulted { get; }

        /// <summary>
        /// A value indicating if the node is hidden.
        /// </summary>
        public bool IsHidden { get; }

        /// <summary>
        /// A value indicating if the node is a root node.
        /// </summary>
        public bool IsRoot { get; }

        /// <summary>
        /// A value indicating if the node is nested
        /// </summary>
        public bool IsNested { get; }

        /// <summary>
        /// The parent node. Is <see langword="null"/> when there is no parent.
        /// </summary>
        public ItemNode? Parent => _parent ??= Create(Item.Parent);

        /// <summary>
        /// A list of child nodes.
        /// </summary>
        public IEnumerable<ItemNode?> Children => _children ??= Item.Children.Select(t => Create(t));

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
        public static async Task<ItemNode?> CreateAsync(IVsHierarchy hierarchy, uint itemId)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsHierarchyItem? item = await hierarchy.ToHierarcyItemAsync(itemId);

            return Create(item);
        }

        /// <summary>
        /// Creates a new instance based on a hierarchy item.
        /// </summary>
        public static ItemNode? Create(IVsHierarchyItem? item)
        {
            if (item == null)
            {
                return null;
            }

            return new ItemNode(item);
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
