using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    public partial class Events
    {
        /// <summary>
        /// Events related to the editor documents.
        /// </summary>
        public BuildEvents BuildEvents { get; } = new();
    }

    /// <summary>
    /// Events related to the editor documents.
    /// </summary>
    public class BuildEvents : IVsUpdateSolutionEvents2
    {
        internal BuildEvents()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsSolutionBuildManager svc = VS.GetRequiredService<SVsSolutionBuildManager, IVsSolutionBuildManager>();
            svc!.AdviseUpdateSolutionEvents(this, out _);
        }

        /// <summary>
        /// Fires when the solution starts building.
        /// </summary>
        public event EventHandler? SolutionBuildStarted;

        /// <summary>
        ///  Fires when the solution is done building.
        /// </summary>
        public event Action<bool>? SolutionBuildDone;

        /// <summary>
        ///  Fires when the solution build was cancelled
        /// </summary>
        public event Action? SolutionBuildCancelled;

        /// <summary>
        /// Fires when a project starts building.
        /// </summary>
        public event Action<Project?>? ProjectBuildStarted;

        /// <summary>
        /// Fires when a project is done building.
        /// </summary>
        public event Action<Project?>? ProjectBuildDone;

        /// <summary>
        /// Fires when a project starts cleaning.
        /// </summary>
        public event Action<Project?>? ProjectCleanStarted;

        /// <summary>
        /// Fires when a project is done cleaning.
        /// </summary>
        public event Action<Project?>? ProjectCleanDone;

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            SolutionBuildStarted?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }
        int IVsUpdateSolutionEvents2.UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            SolutionBuildStarted?.Invoke(this, EventArgs.Empty);
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            SolutionBuildDone?.Invoke(fSucceeded == 0);
            return VSConstants.S_OK;
        }
        int IVsUpdateSolutionEvents2.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            SolutionBuildDone?.Invoke(fSucceeded == 0);
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;
        int IVsUpdateSolutionEvents2.UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;

        int IVsUpdateSolutionEvents2.UpdateSolution_Cancel()
        {
            SolutionBuildCancelled?.Invoke();
            return VSConstants.S_OK;
        }
        int IVsUpdateSolutionEvents.UpdateSolution_Cancel()
        {
            SolutionBuildCancelled?.Invoke();
            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;
        int IVsUpdateSolutionEvents2.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // This method is called when a specific project begins building.

            // if clean project or solution,   dwAction == 0x100000
            // if build project or solution,   dwAction == 0x010000
            // if rebuild project or solution, dwAction == 0x410000
            if (ProjectCleanStarted != null || ProjectBuildStarted != null)
            {
                Project? project = SolutionItem.FromHierarchy(pHierProj, VSConstants.VSITEMID_ROOT) as Project;

                // Clean
                if (dwAction == 0x100000)
                {
                    ProjectCleanStarted?.Invoke(project);
                }
                // Build and rebuild
                else
                {
                    ProjectBuildStarted?.Invoke(project);
                }
            }

            return VSConstants.S_OK;
        }

        int IVsUpdateSolutionEvents2.UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // This method is called when a specific project finishes building.
            if (ProjectBuildDone != null || ProjectCleanDone != null)
            {
                Project? project = SolutionItem.FromHierarchy(pHierProj, VSConstants.VSITEMID_ROOT) as Project;

                // Clean
                if (dwAction == 0x100000)
                {
                    ProjectCleanDone?.Invoke(project);
                }
                // Build and rebuild
                else
                {
                    ProjectBuildDone?.Invoke(project);
                }
            }

            return VSConstants.S_OK;
        }
    }
}
