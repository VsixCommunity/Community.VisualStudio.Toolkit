using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Handles building of solutions and projects.
    /// </summary>
    public class Build
    {
        internal Build()
        { }

        /// <summary>
        /// Cancels the solution build asynchronously
        /// </summary>
        /// <returns>Returns 'true' if successfull</returns>
        public async Task<bool> CancelBuildAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolutionBuildManager svc = await VS.Services.GetSolutionBuildManagerAsync();
            svc.CanCancelUpdateSolutionConfiguration(out int canCancel);

            if (canCancel == 0)
            {
                return false;
            }

            return svc.CancelUpdateSolutionConfiguration() == VSConstants.S_OK;
        }

        /// <summary>
        /// Builds the solution.
        /// </summary>
        public async Task<bool> BuildSolutionAsync(BuildAction action = BuildAction.Build)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolutionBuildManager svc = await VS.Services.GetSolutionBuildManagerAsync();
            uint buildFlags = (uint)GetBuildFlags(action);

            BuildObserver observer = new(null);
            ErrorHandler.ThrowOnFailure(svc.AdviseUpdateSolutionEvents(observer, out uint cookie));

            try
            {
                ErrorHandler.ThrowOnFailure(svc.StartSimpleUpdateSolutionConfiguration(buildFlags, 0, 0));
                return await observer.Result;
            }
            finally
            {
                svc.UnadviseUpdateSolutionEvents(cookie);
            }
        }

        /// <summary>
        /// Builds the specified project.
        /// </summary>
        public async Task<bool> BuildProjectAsync(SolutionItem project, BuildAction action = BuildAction.Build)
        {
            if (project?.Type != SolutionItemType.Project && project?.Type != SolutionItemType.VirtualProject)
            {
                return false;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolutionBuildManager svc = await VS.Services.GetSolutionBuildManagerAsync();
            uint buildFlags = (uint)GetBuildFlags(action);

            project.GetItemInfo(out IVsHierarchy hierarchy, out _, out _);

            BuildObserver observer = new(hierarchy);
            ErrorHandler.ThrowOnFailure(svc.AdviseUpdateSolutionEvents(observer, out uint cookie));

            try
            {
                ErrorHandler.ThrowOnFailure(svc.StartSimpleUpdateProjectConfiguration(hierarchy, null, null, buildFlags, 0, 0));
                return await observer.Result;
            }
            finally
            {
                svc.UnadviseUpdateSolutionEvents(cookie);
            }
        }

        /// <summary>
        /// Checks if the project build is up to date.
        /// </summary>
        public async Task<bool> ProjectIsUpToDateAsync(SolutionItem project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsSolutionBuildManager svc = await VS.Services.GetSolutionBuildManagerAsync();
            IVsProjectCfg2[] projectConfig = new IVsProjectCfg2[1];

            project.GetItemInfo(out IVsHierarchy hierarchy, out _, out _);

            if (ErrorHandler.Succeeded(svc.FindActiveProjectCfg(IntPtr.Zero, IntPtr.Zero, hierarchy, projectConfig)))
            {
                int[] supported = new int[1];
                int[] ready = new int[1];

                if (ErrorHandler.Succeeded(projectConfig[0].get_BuildableProjectCfg(out IVsBuildableProjectCfg buildableProjectConfig)) &&
                    ErrorHandler.Succeeded(buildableProjectConfig.QueryStartUpToDateCheck(0, supported, ready)) &&
                    supported[0] == 1)
                {
                    return ErrorHandler.Succeeded(buildableProjectConfig.StartUpToDateCheck(null, (uint)VsUpToDateCheckFlags.VSUTDCF_DTEEONLY));
                }
            }

            return false;
        }

        private static VSSOLNBUILDUPDATEFLAGS GetBuildFlags(BuildAction action)
        {
            return action switch
            {
                BuildAction.Build => VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD,
                BuildAction.Rebuild => VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD | VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_FORCE_UPDATE,
                BuildAction.Clean => VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_CLEAN,
                _ => VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD,
            };
        }

        private class BuildObserver : IVsUpdateSolutionEvents2
        {
            private readonly TaskCompletionSource<bool> _result = new();
            private readonly IVsHierarchy? _hierarchy;

            public BuildObserver(IVsHierarchy? hierarchy)
            {
                _hierarchy = hierarchy;
            }

            public Task<bool> Result => _result.Task;

            public int UpdateSolution_Begin(ref int pfCancelUpdate) => VSConstants.S_OK;

            public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
            {
                if (_hierarchy is null)
                {
                    // We are watching the build of the entire solution, 
                    // so we can set the result now.
                    if (fCancelCommand != 0)
                    {
                        _result.SetCanceled();
                    }
                    else
                    {
                        _result.SetResult(fSucceeded != 0);
                    }
                }

                return VSConstants.S_OK;
            }

            public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) => VSConstants.S_OK;

            public int UpdateSolution_Cancel() => VSConstants.S_OK;

            public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => VSConstants.S_OK;

            public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel) => VSConstants.S_OK;

            public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
            {
                if (_hierarchy is not null)
                {
                    if (fCancel != 0)
                    {
                        _result.SetCanceled();
                    }
                    // We are observing the build of a specific project. If the project
                    // that finished is the one we are observing, then we can set the result.
                    else if (pHierProj == _hierarchy)
                    {
                        _result.SetResult(fSuccess != 0);
                    }
                }

                return VSConstants.S_OK;
            }
        }
    }

    /// <summary>
    /// The types of build actions for a solution- or project build.
    /// </summary>
    public enum BuildAction
    {
        /// <summary>Builds the solution/project.</summary>
        Build,
        /// <summary>Rebuilds the solution/project.</summary>
        Rebuild,
        /// <summary>Cleans the solution/project.</summary>
        Clean
    }
}
