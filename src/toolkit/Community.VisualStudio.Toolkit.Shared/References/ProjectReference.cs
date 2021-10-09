using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Represents a project reference in a project.
    /// </summary>
    public class ProjectReference : Reference
    {
        internal ProjectReference(IVsReference vsReference) : base(vsReference) { }

        /// <inheritdoc/>
        public override string Name
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                // For some project types, project references don't have a name.
                // When that's the case, we'll get the name of the project ourselves.
                string? name = base.Name;
                if (name is null)
                {
                    IVsSolution solution = VS.GetRequiredService<SVsSolution, IVsSolution>();
                    if (ErrorHandler.Succeeded(solution.GetProjectOfGuid(GetProjectGuid(), out IVsHierarchy hierarchy)))
                    {
                        name = SolutionItem.FromHierarchy(hierarchy, VSConstants.VSITEMID_ROOT)?.Name;
                    }
                }

                return name ?? "";
            }
        }

        /// <summary>
        /// Gets the project that the reference is to.
        /// </summary>
        /// <returns>The project if it exists; otherwise, <see langword="null"/>.</returns>
        public async Task<Project?> GetProjectAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsSolution solution = await VS.Services.GetSolutionAsync();
            if (ErrorHandler.Succeeded(solution.GetProjectOfGuid(GetProjectGuid(), out IVsHierarchy hierarchy)))
            {
                return await SolutionItem.FromHierarchyAsync(hierarchy, VSConstants.VSITEMID_ROOT) as Project;
            }

            return null;
        }

        private Guid GetProjectGuid()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // A `ProjectReference` can represent an `IVsProjectReference` or an `IVsSharedProjectReference`.
            if (VsReference is IVsProjectReference projectReference)
            {
                Guid.TryParse(projectReference.Identity, out Guid guid);
                return guid;
            }
            else
            {
                return ((IVsSharedProjectReference)VsReference).SharedProjectID;
            }
        }
    }
}
