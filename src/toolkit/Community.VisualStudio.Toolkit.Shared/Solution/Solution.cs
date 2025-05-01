using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;


namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents the solution itself.
    /// </summary>
    public class Solution : SolutionItem
    {
        internal Solution(IVsHierarchyItem item, SolutionItemType type) : base(item, type)
        { ThreadHelper.ThrowIfNotOnUIThread(); }

        /// <summary>
        /// Adds a solution folder to the solution
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<SolutionFolder?> AddSolutionFolderAsync(string name)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            Guid guid = new(ProjectTypes.SOLUTION_FOLDER_OTHER);
            Guid iidProject = typeof(IVsHierarchy).GUID;
            IVsSolution sol = await VS.Services.GetSolutionAsync();

            int hr = sol.CreateProject(ref guid, null, null, name, 0, ref iidProject, out IntPtr ptr);

            if (hr == VSConstants.S_OK && ptr != IntPtr.Zero)
            {
                if (Marshal.GetObjectForIUnknown(ptr) is IVsHierarchy hier)
                {
                    if (await FromHierarchyAsync(hier, (uint)VSConstants.VSITEMID.Root) is SolutionItem item)
                    {
                        return (SolutionFolder)item;
                    }
                }

                Marshal.Release(ptr);
            }

            return null;
        }

        /// <summary>
        /// Save the solution if it's dirty.
        /// </summary>
        public async Task SaveAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsSolution solution = await VS.Services.GetSolutionAsync();
            int hr = solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, null, 0);

            ErrorHandler.ThrowOnFailure(hr);
        }
    }
}
