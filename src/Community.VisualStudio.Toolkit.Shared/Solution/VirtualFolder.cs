using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents a virtual folder in the solution hierarchy.
    /// </summary>
    public class VirtualFolder : SolutionItem
    {
        internal VirtualFolder(IVsHierarchyItem item, SolutionItemType type) : base(item, type)
        { ThreadHelper.ThrowIfNotOnUIThread(); }

        /// <summary>
        /// The project containing this folder, or <see langword="null"/>.
        /// </summary>
        public Project? ContainingProject => FindParent(SolutionItemType.Project) as Project;
    }
}
