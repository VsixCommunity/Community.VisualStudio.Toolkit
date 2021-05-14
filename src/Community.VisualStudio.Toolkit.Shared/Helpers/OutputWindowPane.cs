using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit.Shared.Helpers
{
    /// <summary>
    /// A pane in the Output window.
    /// </summary>
    /// <remarks>
    /// OutputWindowPane allows an extension to create a new Output window pane or get an existing one.
    /// A pane can be activated (shown), hidden and cleared. Text can be written to the pane via methods
    /// like <see cref="WriteLineAsync(string)"/> or with a <see cref="TextWriter"/> returned
    /// from <see cref="CreateOutputPaneTextWriterAsync"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// Guid myPaneGuid;
    /// {
    ///     OutputWindowPane pane = await VS.Windows.CreateOutputWindowPaneAsync("My Pane");
    ///     myPaneGuid = pane.Guid;
    ///     await pane.WriteLineAsync("My message");
    /// }
    /// 
    /// // Elsewhere:
    /// {
    ///     OutputWindowPane pane = await VS.Windows.GetOutputWindowPaneAsync(myPaneGuid);
    ///     pane.Name = "My Pane - Updated";
    ///     using (TextWriter writer = await pane.CreateOutputPaneTextWriterAsync())
    ///     {
    ///         char[] buffer = GetSomeChars();
    ///         await writer.WriteLineAsync("This is a more efficient way to write lots of text.");
    ///         await writer.WriteAsync(buffer, 0, buffer.Length);
    ///         await writer.WriteLineAsync();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class OutputWindowPane
    {
        private readonly string _newPaneName;
        private IVsOutputWindowPane? _pane;

        /// <summary>
        /// Creates a new Output window pane with the given name (title).
        /// The new pane can be created at construction time or lazily upon first write.
        /// </summary>
        /// <param name="name">The name (title) of the new pane.</param>
        /// <param name="lazyCreate">Whether to lazily create the pane upon first write.</param>
        /// <returns>A new OutputWindowPane.</returns>
        public static async Task<OutputWindowPane> CreateAsync(string name, bool lazyCreate = true)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(nameof(name));

            var pane = new OutputWindowPane(name, Guid.NewGuid());

            if (!lazyCreate)
            {
                await pane.EnsurePaneAsync();
            }

            return pane;
        }

        /// <summary>
        /// Gets an existing Visual Studio Output window pane (General, Build, Debug).
        /// If the General pane does not already exist then it will be created, but that is not
        /// the case for Build or Debug.
        /// </summary>
        /// <param name="pane">The Visual Studio pane to get.</param>
        /// <returns>A new OutputWindowPane.</returns>
        public static Task<OutputWindowPane> GetAsync(Windows.VSOutputWindowPane pane)
        {
            return pane switch
            {
                Windows.VSOutputWindowPane.General => GetAsync(VSConstants.OutputWindowPaneGuid.GeneralPane_guid),
                Windows.VSOutputWindowPane.Build => GetAsync(VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid),
                Windows.VSOutputWindowPane.Debug => GetAsync(VSConstants.OutputWindowPaneGuid.DebugPane_guid),
                _ => throw new InvalidOperationException("Unexpected VisualStudioPane"),
            };
        }

        /// <summary>
        /// Gets an existing Output window pane.
        /// Throws if a pane with the specified guid does not exist.
        /// </summary>
        /// <param name="guid">The pane's unique identifier.</param>
        /// <returns>A new OutputWindowPane.</returns>
        public static async Task<OutputWindowPane> GetAsync(Guid guid)
        {
            // Empty string for `_name` signals to EnsurePaneAsync that we want to get an existing pane.
            var pane = new OutputWindowPane(string.Empty, guid);

            await pane.EnsurePaneAsync();

            return pane;
        }

        /// <summary>
        /// Uniquely identifies the Output window pane.
        /// After creating a pane, you can cache this Guid and later obtain the same pane from <see cref="GetAsync(Guid)"/>.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// The name (title) of the Output window pane.
        /// </summary>
        public string Name
        {
            get
            {
                return ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    await EnsurePaneAsync();

                    if (_pane == null)
                        throw new InvalidOperationException("IVsOutputWindowPane should exist");

                    var name = string.Empty;
                    _pane.GetName(ref name);
                    return name;
                });
            }

            set
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    await EnsurePaneAsync();

                    if (_pane == null)
                        throw new InvalidOperationException("IVsOutputWindowPane should exist");

                    _pane.SetName(value);
                });
            }
        }

        /// <summary>
        /// Shows and activates the Output window pane.
        /// </summary>
        public async Task ActivateAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await EnsurePaneAsync();

            if (_pane == null)
                throw new InvalidOperationException("IVsOutputWindowPane should exist");

            _pane.Activate();
        }

        /// <summary>
        /// Hides the Output window pane.
        /// </summary>
        public async Task HideAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await EnsurePaneAsync();

            if (_pane == null)
                throw new InvalidOperationException("IVsOutputWindowPane should exist");

            _pane.Hide();
        }

        /// <summary>
        /// Clears the Output window pane.
        /// </summary>
        public async Task ClearAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await EnsurePaneAsync();

            if (_pane == null)
                throw new InvalidOperationException("IVsOutputWindowPane should exist");

            _pane.Clear();
        }

        /// <summary>
        /// Writes a new line to the Output window pane.
        /// </summary>
        public void WriteLine()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await WriteLineAsync();
            });
        }

        /// <summary>
        /// Writes the given text followed by a new line to the Output window pane.
        /// </summary>
        /// <param name="value">The text value to write.</param>
        public void WriteLine(string value)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await WriteLineAsync(value);
            });
        }

        /// <summary>
        /// Writes a new line to the Output window pane.
        /// </summary>
        public Task WriteLineAsync()
        {
            return WriteLineAsync(string.Empty);
        }

        /// <summary>
        /// Writes the given text followed by a new line to the Output window pane.
        /// </summary>
        /// <param name="value">The text value to write. May be an empty string, in which case a newline is written.</param>
        public async Task WriteLineAsync(string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await EnsurePaneAsync();

            if (_pane == null)
                throw new InvalidOperationException("IVsOutputWindowPane should exist");

            _pane.OutputString(value + Environment.NewLine);
        }

        /// <summary>
        /// Returns a new <see cref="TextWriter"/> that can be used to write text to the Output window pane.
        /// For newer versions of Visual Studio this provides a more efficient way to write lots
        /// of text to the pane.
        /// </summary>
        public async Task<TextWriter> CreateOutputPaneTextWriterAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await EnsurePaneAsync();

            if (_pane == null)
                throw new InvalidOperationException("IVsOutputWindowPane should exist");

#if VS16 || VS17
            return new OutputWindowTextWriter(_pane);
#else
            return new OutputWindowTextWriterVS14(_pane);  // For VS15 too
#endif
        }

        private async Task EnsurePaneAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_pane == null)
            {
                // Special case for Visual Studio's General pane
                if (Guid == VSConstants.OutputWindowPaneGuid.GeneralPane_guid)
                {
                    _pane = await VS.GetServiceAsync<SVsGeneralOutputWindowPane, IVsOutputWindowPane>();
                    return;
                }

                IVsOutputWindow outputWindow = await VS.Windows.GetOutputWindowAsync();
                Guid paneGuid = Guid;

                // Only create the pane if we were constructed with a non-empty `_newPaneName`.
                if (!string.IsNullOrEmpty(_newPaneName))
                {
                    const int visible = 1;
                    const int clearWithSolution = 1;
                    ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref paneGuid, _newPaneName, visible, clearWithSolution));
                }

                ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref paneGuid, out _pane));
            }
        }

        private OutputWindowPane(string newPaneName, Guid paneGuid)
        {
            _newPaneName = newPaneName;
            Guid = paneGuid;
        }
    }
}
