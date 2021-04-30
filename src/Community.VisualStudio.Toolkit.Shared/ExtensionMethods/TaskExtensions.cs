using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.Shell
{
    /// <summary>Extension methods for the <see cref="System.Threading.Tasks.Task" /> object.</summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Starts a Task and lets it run in the background, while silently handles any exceptions.
        /// </summary>
        /// <remarks>
        /// This is similar to the <c>task.FileAndForget(string)</c> method introduced in 16.0, but this doesn't record
        /// telemetry on faults and it doesn't take a string parameter. This also works in all version of Visual Studio.
        /// </remarks>
        public static void ForgetAndLog(this System.Threading.Tasks.Task task)
        {
            task.ContinueWith(delegate (System.Threading.Tasks.Task antecedent)
            {
                antecedent.Exception!.Log();

            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default).Forget();
        }
    }
}
