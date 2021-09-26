using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace TestExtension
{
    [SolutionTreeFilterProvider(PackageGuids.TestExtensionString, PackageIds.VsixManifestSolutionExplorerFilter)]
    public class VsixManifestFilterProvider : HierarchyTreeFilterProvider
    {
        private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;

        [ImportingConstructor]
        public VsixManifestFilterProvider(IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
        {
            _hierarchyCollectionProvider = hierarchyCollectionProvider;
        }

        protected override HierarchyTreeFilter CreateFilter()
        {
            return new Filter(_hierarchyCollectionProvider);
        }

        private sealed class Filter : HierarchyTreeFilter
        {
            private static readonly Regex _pattern = new Regex(@"\.vsixmanifest$", RegexOptions.IgnoreCase);

            private readonly IVsHierarchyItemCollectionProvider _hierarchyCollectionProvider;

            public Filter(IVsHierarchyItemCollectionProvider hierarchyCollectionProvider)
            {
                _hierarchyCollectionProvider = hierarchyCollectionProvider;
            }

            protected override async Task<IReadOnlyObservableSet> GetIncludedItemsAsync(IEnumerable<IVsHierarchyItem> rootItems)
            {
                IVsHierarchyItem root = HierarchyUtilities.FindCommonAncestor(rootItems);
                IReadOnlyObservableSet<IVsHierarchyItem> sourceItems;

                sourceItems = await _hierarchyCollectionProvider.GetDescendantsAsync(root.HierarchyIdentity.NestedHierarchy, CancellationToken);

                return await _hierarchyCollectionProvider.GetFilteredHierarchyItemsAsync(sourceItems, MeetsFilter, CancellationToken);
            }

            private static bool MeetsFilter(IVsHierarchyItem item)
            {
                return (item != null) && _pattern.IsMatch(item.Text);
            }
        }
    }
}