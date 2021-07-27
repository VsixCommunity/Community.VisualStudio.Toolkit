using System;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A <see cref="System.IO.TextWriter"/> that writes to an Output window pane.
    /// This is suitable for use in Visual Studio 2015 and 2017 where OutputWindowTextWriter is not available.
    /// Overrides the same methods on TextWriter that OutputWindowTextWriter does.
    /// </summary>
    internal class OutputWindowTextWriterVS14 : System.IO.TextWriter
    {
        private readonly IVsOutputWindowPane _pane;

        /// <summary>
        /// Creates the text writer.
        /// </summary>
        /// <param name="pane">The Output window pane.</param>
        public OutputWindowTextWriterVS14(IVsOutputWindowPane pane)
        {
            _pane = pane;
        }

        /// <summary>
        /// The character encoding in which the output is written.
        /// </summary>
        public override Encoding Encoding => Encoding.Default;

        private void Output(string value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_pane is IVsOutputWindowPaneNoPump nopump)
            {
                nopump.OutputStringNoPump(value);
            }
            else
            {
                ErrorHandler.ThrowOnFailure(_pane.OutputStringThreadSafe(value));
            }
        }

        #region Write

        /// <summary>
        /// Writes text to the Output window pane.
        /// </summary>
        /// <param name="value">The text to write.</param>
        public override void Write(string value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Output(value);
        }

        /// <summary>
        /// Writes an array of characters to the Output window pane.
        /// </summary>
        /// <param name="buffer">The character array.</param>
        /// <param name="index">Start index.</param>
        /// <param name="count">Number of characters,</param>
        public override void Write(char[] buffer, int index, int count)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Output(new string(buffer, index, count));
        }

        #endregion

        #region WriteAsync

        /// <summary>
        /// Writes text to the Output window pane.
        /// </summary>
        /// <param name="value">The text to write.</param>
        public override async Task WriteAsync(string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Output(value);
        }

        /// <summary>
        /// Writes a character to the Output window pane.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public override async Task WriteAsync(char value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Output(value.ToString());
        }

        /// <summary>
        /// Writes an array of characters to the Output window pane.
        /// </summary>
        /// <param name="buffer">The character array.</param>
        /// <param name="index">Start index.</param>
        /// <param name="count">Number of characters,</param>
        public override async Task WriteAsync(char[] buffer, int index, int count)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Output(new string(buffer, index, count));
        }

        #endregion

        #region WriteLine

        public override void WriteLine(string value)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Output(value + Environment.NewLine);
        }

        #endregion

        #region WriteLineAsync

        /// <summary>
        /// Writes an empty new line to the Output window pane.
        /// </summary>
        public override async Task WriteLineAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Output(Environment.NewLine);
        }

        /// <summary>
        /// Writes text followed by a new line to the Output window pane.
        /// </summary>
        /// <param name="value">The text to write.</param>
        public override async Task WriteLineAsync(string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Output(value + Environment.NewLine);
        }

        /// <summary>
        /// Writes a character followed by a new line to the Output window pane.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public override async Task WriteLineAsync(char value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Output(value.ToString() + Environment.NewLine);
        }

        /// <summary>
        /// Writes an array of characters followed by a new line to the Output window pane.
        /// </summary>
        /// <param name="buffer">The character array.</param>
        /// <param name="index">Start index.</param>
        /// <param name="count">Number of characters,</param>
        public override async Task WriteLineAsync(char[] buffer, int index, int count)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Output(new string(buffer, index, count) + Environment.NewLine);
        }

        #endregion
    }
}
