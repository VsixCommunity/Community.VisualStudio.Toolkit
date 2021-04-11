using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace System
{
    /// <summary>Extension methods for the Exception class.</summary>
    public static class ExceptionExtensions
    {
        private const string _paneTitle = "Extensions";

        private static IVsOutputWindowPane? _pane;

        /// <summary>
        /// Log the error to the Output Window
        /// </summary>
        /// <remarks>
        /// It creates a new Output Window pane called "Extensions" where it logs to. This is to no polute
        /// the existing "Build" pane with errors coming from extensions.
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     // Do work;
        /// }
        /// catch (Exception ex)
        /// {
        ///     ex.Log();
        /// }
        /// </code>
        /// </example>
        public static void Log(this Exception exception)
        {
            try
            {
                LogAsync(exception).FireAndForget();
            }
            catch (Exception ex)
            {
                Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Log the error to the Output Window asyncronously.
        /// </summary>
        /// <remarks>
        /// It creates a new Output Window pane called "Extensions" where it logs to. This is to no polute
        /// the existing "Build" pane with errors coming from extensions.
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     // Do work;
        /// }
        /// catch (Exception ex)
        /// {
        ///     await ex.LogAsync();
        /// }
        /// </code>
        /// </example>
        public static async Task LogAsync(this Exception exception)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                if (await EnsurePaneAsync())
                {
                    _pane?.OutputStringThreadSafe(exception + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Debug.WriteLine(ex);
            }
        }

        private static async Task<bool> EnsurePaneAsync()
        {
            if (_pane == null)
            {
                try
                {
                    if (_pane == null)
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                        IVsOutputWindow output = await VS.Windows.GetOutputWindowAsync();
                        var guid = new Guid();

                        ErrorHandler.ThrowOnFailure(output.CreatePane(ref guid, _paneTitle, 1, 1));
                        ErrorHandler.ThrowOnFailure(output.GetPane(ref guid, out _pane));
                    }
                }
                catch (Exception ex)
                {
                    Diagnostics.Debug.WriteLine(ex);
                }
            }

            return _pane != null;
        }
    }
}