using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents a file, folder, project, or other item in Solution Explorer.
    /// </summary>
    [DebuggerDisplay("{Name} ({Type})")]
    public class SolutionItem
    {
        private SolutionItem? _parent;
        private IEnumerable<SolutionItem?>? _children;
        private IVsHierarchyItem _item = default!; // Initialized to non-null via the `Update()` method.
        private IVsHierarchy _hierarchy = default!; // Initialized to non-null via the `Update()` method.
        private string? _fullPath;
        private uint _itemId;
        private Lazy<bool> _isNonVisibleItem = default!; // Initialized to non-null via the `Update()` method.

        /// <summary>
        /// Creates a new instance of the solution item.
        /// </summary>
        protected SolutionItem(IVsHierarchyItem item, SolutionItemType type)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Type = type;
            Update(item);
        }

        internal void Update(IVsHierarchyItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _item = item;
            _hierarchy = item.GetHierarchy();
            _itemId = item.GetItemId();
            _fullPath = GetFullPath();
            _isNonVisibleItem = new Lazy<bool>(() =>
            {
                return HierarchyUtilities.TryGetHierarchyProperty(_hierarchy, _itemId, (int)__VSHPROPID.VSHPROPID_IsNonMemberItem, out bool isNonMemberItem) && isNonMemberItem;
            });
        }

        /// <summary>
        /// The name of the item.
        /// </summary>
        public string Name => _item.CanonicalName;

        /// <summary>
        /// The display text of the item.
        /// </summary>
        public string Text => _item.Text;

        /// <summary>
        /// The absolute file path on disk.
        /// </summary>
        public string? FullPath => _fullPath;

        /// <summary>
        /// The type of solution item.
        /// </summary>
        public SolutionItemType Type { get; }

        /// <summary>
        /// The parent item. Is <see langword="null"/> when there is no parent.
        /// </summary>
        public SolutionItem? Parent => _parent ??= FromHierarchyItem(_item.Parent);

        /// <summary>
        /// A list of child items.
        /// </summary>
        public IEnumerable<SolutionItem?> Children => _children ??= _item.Children.Select(t => FromHierarchyItem(t));

        /// <summary>
        /// Returns whether the item is normally hidden in solution explorer and only visible when Show All Files is enabled.
        /// </summary>
        public bool IsNonVisibleItem => _isNonVisibleItem.Value;

        /// <summary>
        /// Gets information from the underlying data types.
        /// </summary>
        public void GetItemInfo(out IVsHierarchy hierarchy, out uint itemId, out IVsHierarchyItem hierarchyItem)
        {
            hierarchy = _hierarchy;
            itemId = _itemId;
            hierarchyItem = _item;
        }

        /// <summary>
        /// Finds the nearest parent matching the specified type.
        /// </summary>
        public SolutionItem? FindParent(SolutionItemType type)
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
        /// Creates a new instance based on a hierarchy.
        /// </summary>
        public static async Task<SolutionItem?> FromHierarchyAsync(IVsHierarchy hierarchy, uint itemId)
        {
            if (hierarchy is null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsHierarchyItem item = await hierarchy.ToHierarchyItemAsync(itemId);

            return FromHierarchyItem(item);
        }

        /// <summary>
        /// Creates a new instance based on a hierarchy.
        /// </summary>
        public static SolutionItem? FromHierarchy(IVsHierarchy hierarchy, uint itemId)
        {
            if (hierarchy is null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            IVsHierarchyItem item = hierarchy.ToHierarchyItem(itemId);

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

            SolutionItemType type = GetSolutionItemType(item.HierarchyIdentity);

            try
            {
                return type switch
                {
                    SolutionItemType.Solution => new Solution(item, type),
                    SolutionItemType.Project => new Project(item, type),
                    SolutionItemType.PhysicalFile => new PhysicalFile(item, type),
                    SolutionItemType.PhysicalFolder => new PhysicalFolder(item, type),
                    SolutionItemType.VirtualFolder => new VirtualFolder(item, type),
                    SolutionItemType.SolutionFolder => new SolutionFolder(item, type),
                    _ => new SolutionItem(item, type)
                };
            }
            catch
            {
                // If we failed to create the item, we should return null.
                return null;
            }
        }

        /// <summary>
        /// Selects the given hierarchy/itemId in Solution Explorer.
        /// Pass the item-specific hierarchy if you have both (hierarchy & itemHierarchy).
        /// </summary>
        public static async System.Threading.Tasks.Task SelectInSolutionExplorerAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_hierarchy == null)
                return;

            // Ensure Solution Explorer is created/shown
            var uiShellObj = await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(SVsUIShell));
            var seGuid = VSConstants.StandardToolWindows.SolutionExplorer;
            IVsWindowFrame? frame = null;
            if (uiShellObj is IVsUIShell uiShell)
            {
                uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref seGuid, out frame);
            }

            if (frame == null)
                return;

            frame.Show();

            // Get the IVsUIHierarchyWindow behind Solution Explorer
            frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object docView);
            if (docView is not IVsUIHierarchyWindow hierarchyWindow)
                return;

            if (hierarchy is IVsUIHierarchy uiHierarchy)
            {
                hierarchyWindow.ExpandItem(uiHierarchy, _itemId, EXPANDFLAGS.EXPF_SelectItem);
            }
        }

        private static SolutionItemType GetSolutionItemType(IVsHierarchyItemIdentity identity)
        {
            if (HierarchyUtilities.IsSolutionNode(identity))
            {
                return SolutionItemType.Solution;
            }
            else if (HierarchyUtilities.IsSolutionFolder(identity))
            {
                return SolutionItemType.SolutionFolder;
            }
            else if (HierarchyUtilities.IsMiscellaneousProject(identity))
            {
                return SolutionItemType.MiscProject;
            }
            else if (HierarchyUtilities.IsVirtualProject(identity))
            {
                return SolutionItemType.VirtualProject;
            }
            else if (HierarchyUtilities.IsProject(identity))
            {
                return SolutionItemType.Project;
            }
            else if (HierarchyUtilities.IsStubHierarchy(identity))
            {
                // This is most likely an unloaded project.
                return SolutionItemType.Project;
            }
            else if (HierarchyUtilities.IsPhysicalFile(identity))
            {
                return SolutionItemType.PhysicalFile;
            }
            else if (HierarchyUtilities.IsPhysicalFolder(identity))
            {
                return SolutionItemType.PhysicalFolder;
            }

            Guid itemType = HierarchyUtilities.GetItemType(identity);
            if (itemType == VSConstants.ItemTypeGuid.VirtualFolder_guid)
            {
                return SolutionItemType.VirtualFolder;
            }

            return SolutionItemType.Unknown;
        }

        private string? GetFullPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (Type == SolutionItemType.Unknown || IsVirtualItem(Type))
            {
                return null;
            }

            ErrorHandler.ThrowOnFailure(_hierarchy.GetCanonicalName(_itemId, out string? fileName));

            if (_hierarchy is IVsProject project && project.GetMkDocument(_itemId, out fileName) == VSConstants.S_OK)
            {
                return fileName;
            }

            if (_hierarchy is IVsSolution solution && solution.GetSolutionInfo(out _, out string? slnFile, out _) == VSConstants.S_OK)
            {
                return slnFile;
            }

            return fileName;
        }

        private static bool IsVirtualItem(SolutionItemType type)
        {
            return type switch
            {
                SolutionItemType.SolutionFolder => true,
                SolutionItemType.VirtualProject => true,
                SolutionItemType.VirtualFolder => true,
                _ => false
            };
        }
    }
}
