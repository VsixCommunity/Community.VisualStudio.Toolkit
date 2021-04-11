using System;

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
        public static void FireAndForget(this System.Threading.Tasks.Task task)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await ex.LogAsync();
                }
            });
        }
    }
}
