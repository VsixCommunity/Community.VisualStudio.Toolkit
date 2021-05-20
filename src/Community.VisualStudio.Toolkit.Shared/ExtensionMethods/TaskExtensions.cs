using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.Shell
{
    /// <summary>
    /// Extension methods for <see cref="System.Threading.Tasks.Task" /> and <see cref="JoinableTask" />.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Logs error information to the Output Window if the given <see cref="System.Threading.Tasks.Task" /> faults.
        /// </summary>
        /// <remarks>
        /// The task itself starts before this extension method is called and only continues in
        /// this extension method if the original task throws an unhandled exception.
        /// This is similar to the <c>task.Forget()</c> method, but it logs any unhandled exception
        /// thrown from the task to the Output Window.
        /// </remarks>
        public static void FireAndForget(this System.Threading.Tasks.Task task, bool logOnFailure = true)
        {
            task.ContinueWith(delegate (System.Threading.Tasks.Task antecedent)
            {
                if (logOnFailure)
                {
                    antecedent.Exception!.Log();
                }

            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default).Forget();
        }

        /// <summary>
        /// Logs error information to the Output Window if the given <see cref="JoinableTask" /> faults.
        /// </summary>
        /// <remarks>
        /// This is the JoinableTask equivalent of <see cref="FireAndForget(System.Threading.Tasks.Task, bool)"/>
        /// </remarks>
        public static void FireAndForget(this JoinableTask joinableTask, bool logOnFailure = true)
        {
            FireAndForget(joinableTask.Task, logOnFailure);
        }
    }
}
