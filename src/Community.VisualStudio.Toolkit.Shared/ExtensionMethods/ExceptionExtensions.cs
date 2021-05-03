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
        /// It creates a new Output Window pane called "Extensions" where it logs to. This is to not pollute
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
            LogAsync(exception).Forget();
        }

        /// <summary>
        /// Log the error to the Output Window, along with a formatted string.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks>
        /// It creates a new Output Window pane called "Extensions" where it logs to. This is to not pollute
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
        ///     ex.Log("Failed, parameter was {0}", someParameter);
        /// }
        /// </code>
        /// </example>
        public static void Log(this Exception exception, string format, params object?[] args)
        {
            LogAsync(exception, format, args).Forget();
        }

        /// <summary>
        /// Log the error to the Output Window, along with a message.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message">A message to log along with the exception.</param>
        /// <remarks>
        /// It creates a new Output Window pane called "Extensions" where it logs to. This is to not pollute
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
        ///     ex.Log("Some Message to log with the exception.");
        /// }
        /// </code>
        /// </example>
        public static void Log(this Exception exception, string message)
        {
            LogAsync(exception, message).Forget();
        }

        /// <summary>
        /// Log the error to the Output Window asynchronously.
        /// </summary>
        /// <remarks>
        /// It creates a new Output Window pane called "Extensions" where it logs to. This is to not pollute
        /// the existing "Build" pane with errors coming from extensions.
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     // Do work;
        /// }
        /// catch (Exception ex)`
        /// {
        ///     await ex.LogAsync();
        /// }
        /// </code>
        /// </example>
        public static Task LogAsync(this Exception exception)
        {
            return LogAsync(exception, string.Empty);
        }

        /// <summary>
        /// Log the error to the Output Window asynchronously, along with a formatted string.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks>
        /// It creates a new Output Window pane called "Extensions" where it logs to. This is to not pollute
        /// the existing "Build" pane with errors coming from extensions.
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     // Do work;
        /// }
        /// catch (Exception ex)`
        /// {
        ///     await ex.LogAsync("Failed, parameter was {0}", someParameter);
        /// }
        /// </code>
        /// </example>
        public static Task LogAsync(this Exception exception, string format, params object?[] args)
        {
            string message = format;
            try
            {
                message = string.Format(format, args);
            }
            catch { }

            return LogAsync(exception, message);
        }

        /// <summary>
        /// Log the error to the Output Window asynchronously, along with a message.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="message">A message to log along with the exception.</param>
        /// <remarks>
        /// It creates a new Output Window pane called "Extensions" where it logs to. This is to not pollute
        /// the existing "Build" pane with errors coming from extensions.
        /// </remarks>
        /// <example>
        /// <code>
        /// try
        /// {
        ///     // Do work;
        /// }
        /// catch (Exception ex)`
        /// {
        ///     await ex.LogAsync("Some Message to log with the exception.");
        /// }
        /// </code>
        /// </example>
        public static async Task LogAsync(this Exception exception, string message)
        {
            if (string.IsNullOrEmpty(message))
                message = exception?.ToString() ?? string.Empty;
            else
                message += Environment.NewLine + exception;

            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (await EnsurePaneAsync())
                {
                    //TODO: use OutputWindowTextWriter to write more efficiently. #ifdef it for 14.0
                    _pane?.OutputString(message + Environment.NewLine);
                }
                else
                {
                    Diagnostics.Debug.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Diagnostics.Debug.WriteLine(message);
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