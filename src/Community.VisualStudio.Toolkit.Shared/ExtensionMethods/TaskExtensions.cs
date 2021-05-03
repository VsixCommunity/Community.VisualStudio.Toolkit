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
        /// This is similar to the <c>task.Forget()</c> method, but it logs any unhandled exception
        /// thrown from the task to the Output Window.
        /// </remarks>
        public static void ForgetAndLogOnFailure(this System.Threading.Tasks.Task task)
        {
            task.ContinueWith(delegate (System.Threading.Tasks.Task antecedent)
            {
                antecedent.Exception!.Log();

            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default).Forget();
        }
    }
}
