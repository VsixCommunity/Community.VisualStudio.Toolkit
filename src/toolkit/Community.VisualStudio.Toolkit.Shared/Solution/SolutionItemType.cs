namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Types of items in the solution.
    /// </summary>
    public enum SolutionItemType
    {
        /// <summary>A physical file on disk.</summary>
        PhysicalFile,
        /// <summary>A physical folder on disk.</summary>
        PhysicalFolder,
        /// <summary>A project.</summary>
        Project,
        /// <summary>A miscellaneous project.</summary>
        MiscProject,
        /// <summary>A virtual project.</summary>
        VirtualProject,
        /// <summary>The solution.</summary>
        Solution,
        /// <summary>A solution folder.</summary>
        SolutionFolder,
        /// <summary>An unknown item.</summary>
        Unknown,
        /// <summary>A virtual folder.</summary>
        VirtualFolder,
    }
}
