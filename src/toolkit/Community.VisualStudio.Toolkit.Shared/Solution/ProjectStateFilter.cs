using System;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Defines the different project states that can be used to filter projects.
    /// </summary>
    [Flags]
    public enum ProjectStateFilter
    {
        /// <summary>
        /// No projects.
        /// </summary>
        None = 0,
        /// <summary>
        /// Projects that are currently loaded.
        /// </summary>
        Loaded = 1,
        /// <summary>
        /// Projects that are currently unloaded.
        /// </summary>
        Unloaded = 2,
        /// <summary>
        /// Both loaded and unloaded projects.
        /// </summary>
        All = Loaded | Unloaded
    }
}
