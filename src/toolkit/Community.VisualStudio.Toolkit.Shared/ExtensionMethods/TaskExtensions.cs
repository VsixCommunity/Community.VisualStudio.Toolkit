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

        /// <summary>
        /// Schedules a delegate for background execution on the UI thread without inheriting any claim to the UI thread from its caller.
        /// </summary>
        /// <remarks>
        /// StartOnIdle is a included in later versions of the SDK, but this shim is to add support to VS 14+
        /// </remarks>
        public static JoinableTask StartOnIdleShim(this JoinableTaskFactory joinableTaskFactory, Action action, VsTaskRunContext priority = VsTaskRunContext.UIThreadBackgroundPriority)
        {
            using (joinableTaskFactory.Context.SuppressRelevance())
            {
                return joinableTaskFactory.RunAsync(priority, async delegate
                {
                    await System.Threading.Tasks.Task.Yield();
                    await joinableTaskFactory.SwitchToMainThreadAsync();
                    action();
                });
            }
        }
    }
}
