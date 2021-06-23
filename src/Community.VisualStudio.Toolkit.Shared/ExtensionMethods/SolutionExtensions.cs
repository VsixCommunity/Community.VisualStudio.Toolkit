using System;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

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

            DTE2? dte = await VS.GetDTEAsync();

            if (VS.Events.BuildEvents != null)
            {
                VS.Events.BuildEvents.SolutionBuildDone += BuildEvents_OnBuildDone;
                dte.Solution.SolutionBuild.Build(false);

                void BuildEvents_OnBuildDone(object sender, bool e)
                {
                    VS.Events.BuildEvents.SolutionBuildDone -= BuildEvents_OnBuildDone;

                    // Returns 'true' if the number of failed projects == 0
                    buildTaskCompletionSource.TrySetResult(dte.Solution.SolutionBuild.LastBuildInfo == 0);
                }
            }

            return await buildTaskCompletionSource.Task;
        }

        /// <summary>
        /// Saves the solution.
        /// Although this method is asynchronous, the saving of the solution is still synchronous and is performed on the main UI thread.
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public static async Task SaveAsync(this Solution solution)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await solution.SaveAsAsync(solution.FileName);
        }

        /// <summary>
        /// Saves the solution with the specified name.
        /// Although this method is asynchronous, the saving of the solution is still synchronous and is performed on the main UI thread.
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task SaveAsAsync(this Solution solution, string fileName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (solution == null)
                throw new ArgumentNullException(nameof(solution));

            solution.SaveAs(fileName);
        }
    }
}
