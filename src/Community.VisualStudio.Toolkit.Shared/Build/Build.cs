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
            svc.CanCancelUpdateSolutionConfiguration(out var canCancel);

            if (canCancel == 0)
            {
                return false;
            }

            return svc.CancelUpdateSolutionConfiguration() == VSConstants.S_OK;
        }

        /// <summary>
        /// Builds the solution or project if one is specified.
        /// </summary>
        public async Task<bool> BuildSolutionAsync(BuildAction action = BuildAction.Build)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolutionBuildManager svc = await VS.Services.GetSolutionBuildManagerAsync();
            var buildFlags = (uint)GetBuildFlags(action);

            return svc.StartSimpleUpdateSolutionConfiguration(buildFlags, 0, 0) == VSConstants.S_OK;
        }

        /// <summary>
        /// Builds the solution or project if one is specified.
        /// </summary>
        public async Task<bool> BuildProjectAsync(SolutionItem project, BuildAction action = BuildAction.Build)
        {
            if (project?.Type != NodeType.Project && project?.Type != NodeType.VirtualProject)
            {
                return false;
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolutionBuildManager svc = await VS.Services.GetSolutionBuildManagerAsync();
            var buildFlags = (uint)GetBuildFlags(action);

            project.GetItemInfo(out IVsHierarchy hierarchy, out _, out _);
            return svc.StartSimpleUpdateProjectConfiguration(hierarchy, null, null, buildFlags, 0, 0) == VSConstants.S_OK;
        }

        /// <summary>
        /// Checks if the project build is up to date.
        /// </summary>
        public async Task<bool> ProjectIsUpToDateAsync(SolutionItem project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsSolutionBuildManager svc = await VS.Services.GetSolutionBuildManagerAsync();
            var projectConfig = new IVsProjectCfg2[1];

            project.GetItemInfo(out IVsHierarchy hierarchy, out _, out _);

            if (ErrorHandler.Succeeded(svc.FindActiveProjectCfg(IntPtr.Zero, IntPtr.Zero, hierarchy, projectConfig)))
            {
                var supported = new int[1];
                var ready = new int[1];

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
