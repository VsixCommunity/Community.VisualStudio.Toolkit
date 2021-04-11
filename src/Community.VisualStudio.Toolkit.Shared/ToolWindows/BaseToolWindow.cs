using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class that makes it easier to use tool windows. 
    /// </summary>
    /// <example>
    /// <code>
    /// public class TestToolWindow : BaseToolWindow&lt;TestToolWindow&gt;
    /// {
    ///     public override string GetTitle(int toolWindowId) => "Test Window";
    ///     
    ///     public override Type PaneType => typeof(Pane);
    ///     
    ///     public override async Task&lt;FrameworkElement&gt; CreateAsync(int toolWindowId, CancellationToken cancellationToken)
    ///     {
    ///         await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
    ///         var dte = await VS.GetDTEAsync();
    ///         return new TestWindowControl(dte);
    ///     }
    ///     
    ///     [Guid("d0050678-2e4f-4a93-adcb-af1370da941d")]
    ///     public class Pane : ToolWindowPane
    ///     {
    ///         public Pane()
    ///         {
    ///             BitmapImageMoniker = KnownMonikers.Test;
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="T">The implementation type itself.</typeparam>
    public abstract class BaseToolWindow<T> : IToolWindowProvider where T : BaseToolWindow<T>, new()
    {
        private static ToolkitPackage? _package;
        private static IToolWindowProvider? _implementation;

        /// <summary>
        /// Initializes the tool window. This method must be called from the <see cref="AsyncPackage.InitializeAsync"/> method for the tool window to work.
        /// </summary>
        public static void Initialize(ToolkitPackage package)
        {
            if (_implementation is not null)
            {
                throw new InvalidOperationException($"The tool window '{typeof(T).Name}' has already been initialized.");
            }

            _package = package;
            _implementation = new T() { Package = package };
            package.AddToolWindow(_implementation);
        }

        /// <summary>
        /// Shows the tool window. The tool window will be created if it does not already exist.
        /// </summary>
        /// <param name="id">The ID of the instance of the tool window for multi-instance tool windows.</param>
        /// <param name="create">Whether to create the tool window if it does not already exist.</param>
        /// <returns>A task that returns the <see cref="ToolWindowPane"/> if the tool window already exists or was created, or a task that returns null if the tool window does not exist and was not created.</returns>
        public static async Task<ToolWindowPane?> ShowAsync(int id = 0, bool create = true)
        {
            if (_implementation is null || _package is null)
            {
                throw new InvalidOperationException($"The tool window '{typeof(T).Name}' has not been initialized.");
            }

#if VS16
            return await _package.ShowToolWindowAsync(_implementation.PaneType, id, create, _package.DisposalToken);
#else
            ToolWindowPane window = _package.FindToolWindow(_implementation.PaneType, id, create);

            if (window?.Frame is null)
            {
                if (create)
                {
                    throw new NotSupportedException($"Cannot create the tool window '{_implementation.GetType().Name}'.");
                }
                else
                {
                    return null;
                }
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var windowFrame = (Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return window;
#endif
        }

        /// <summary>
        /// The package class that initialized this class.
        /// </summary>
        protected ToolkitPackage? Package { get; private set; }

        /// <summary>
        /// Gets the title to show in the tool window.
        /// </summary>
        /// <param name="toolWindowId">The ID of the tool window for a multi-instance tool window.</param>
        public abstract string GetTitle(int toolWindowId);

        /// <summary>
        /// Gets the type of <see cref="ToolWindowPane"/> that will be created for this tool window.
        /// </summary>
        public abstract Type PaneType { get; }

        /// <summary>
        /// Creates the UI element that will be shown in the tool window. 
        /// Use this method to create the user control or any other UI element that you want to show in the tool window.
        /// </summary>
        /// <param name="toolWindowId">The ID of the tool window instance being created for a multi-instance tool window.</param>
        /// <param name="cancellationToken">The cancellation token to use when performing asynchronous operations.</param>
        /// <returns>The UI element to show in the tool window.</returns>
        public abstract Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken);
    }
}
