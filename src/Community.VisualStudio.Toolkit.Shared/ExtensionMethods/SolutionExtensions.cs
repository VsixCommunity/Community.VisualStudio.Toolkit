using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace EnvDTE
{
    /// <summary>Extension methods for the Solution class.</summary>
    public static class SolutionExtensions
    {
        /// <summary>
        /// Builds the solution asynchronously
        /// </summary>
        /// <param name="solution"></param>
        /// <returns>Returns 'true' if successfull</returns>
        public static async Task<bool> BuildAsync(this Solution solution)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var buildTaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            DTE? dte = solution.DTE;
            dte.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            var configuration = dte.Solution.SolutionBuild.ActiveConfiguration.Name;

            dte.Solution.SolutionBuild.Build(false);
            return await buildTaskCompletionSource.Task;

            void BuildEvents_OnBuildDone(vsBuildScope scope, vsBuildAction action)
            {
                dte.Events.BuildEvents.OnBuildDone -= BuildEvents_OnBuildDone;

                // Returns 'true' if the number of failed projects == 0
                buildTaskCompletionSource.TrySetResult(dte.Solution.SolutionBuild.LastBuildInfo == 0);
            }
        }
    }
}
