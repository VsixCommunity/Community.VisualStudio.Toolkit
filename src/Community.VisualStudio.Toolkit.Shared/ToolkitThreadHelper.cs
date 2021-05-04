using System;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace Community.VisualStudio.Toolkit.Shared
{
    /// <summary>
    /// Provides a JoinableTaskFactory that is safe to use in MEF components or when an AsyncPackage.JoinableTaskFactory
    /// is not available.
    /// </summary>
    /// <remarks>
    /// JoinableTaskFactory (hereafter 'JTF') from ThreadHelper is not the same as from an AsyncPackage. If you have an
    /// AsyncPackage available then always prefer to use its JTF instance over ThreadHelper. AsyncPackage's JTF will
    /// track all unjoined tasks and ensure they get joined when the package disposes before VS shuts down the CLR.
    /// 
    /// In addition, AsyncPackage provides a disposal token so if you pass that token to all your async work (which you
    /// should) then VS can signal the token to cancel all work and ensure shutdown happens quickly, instead of having
    /// to wait for unfinished tasks to complete.
    ///
    /// But in some places (e.g. MEF components) you won't have an AsyncPackage available and may be tempted to use
    /// ThreadHelper's JTF. That's fine for Run and SwitchToMainThreadAsync and also for RunAsync as long as you
    /// await/join all tasks that RunAsync returns (although you still won't have a disposal token). But if you start
    /// fire-and-forget style tasks using ThreadHelper.JoinableTaskFactory.RunAsync and never explicitly await/join them
    /// then these tasks will never be joined.
    /// 
    /// ToolkitThreadHelper solves the above by creating a new JTF instance along with a collection and a disposal token,
    /// and ensures that all unfinished tasks are joined during disposal, just like AsyncPackage.
    /// 
    /// The implementation is based on aarnott's example here:
    /// https://github.com/microsoft/vs-threading/blob/main/doc/cookbook_vs.md#void-returning-fire-and-forget-methods
    /// </remarks>
    /// <example>
    /// <code>
    /// [Export(typeof(IWpfTextViewCreationListener))]
    /// class TextviewCreationListener : IWpfTextViewCreationListener, IDisposable
    /// {
    ///     private readonly ToolkitThreadHelper _threadHelper = new(ThreadHelper.JoinableTaskContext);
    ///     
    ///     public void Dispose()
    ///     {
    ///         Dispose(true);
    ///         GC.SuppressFinalize(this);
    ///     }
    ///     
    ///     protected virtual void Dispose(bool disposing)
    ///     {
    ///         if (disposing)
    ///         {
    ///             _threadHelper.Dispose();
    ///         }   
    ///     }
    /// 
    ///     public void TextViewCreated(IWpfTextView textView)
    ///     {
    ///         // Note: RunAsync not awaited
    ///         _threadHelper.JoinableTaskFactory.RunAsync(async () =>
    ///         {
    ///             await _threadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_threadHelper.DisposalToken);
    ///                 
    ///             await MyMethodAsync(_threadHelper.DisposalToken);
    ///                 
    ///         }).ForgetAndLogOnFailure();
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ToolkitThreadHelper : IDisposable
    {
        private readonly CancellationTokenSource _disposeCancellationTokenSource = new();
        private readonly JoinableTaskContext _context;
        private readonly JoinableTaskCollection _collection;
        private bool _disposed = false;

        /// <summary>
        /// Creates the ToolkitThreadHelper with Visual Studio's singleton instance of the <see cref="JoinableTaskContext"/>.
        /// This is suitable for use within a Visual Studio extension or unit tests which use the Visual Studio SDK Test Framework.
        /// For all other scenarios, call <see cref="CreateWithContext"/>.
        /// </summary>
        /// <returns>A new ToolkitThreadHelper instance.</returns>
        public static ToolkitThreadHelper Create()
        {
            return new ToolkitThreadHelper(ThreadHelper.JoinableTaskContext);
        }

        /// <summary>
        /// Creates the ToolkitThreadHelper with the supplied <see cref="JoinableTaskContext"/>.
        /// </summary>
        /// <param name="joinableTaskContext">
        /// The application's one-and-only <see cref="JoinableTaskContext"/>.
        /// For Visual Studio extensions, prefer to call <see cref="Create"/> or pass ThreadHelper.JoinableTaskContext,
        /// which is Visual Studio's singleton instance.
        /// For other scenarios, including unit tests which don't use the Visual Studio SDK Test Framework, create a
        /// JoinableTaskContext instance (only one instance for the process) to supply here.
        /// </param>
        /// <returns>A new ToolkitThreadHelper instance.</returns>
        public static ToolkitThreadHelper CreateWithContext(JoinableTaskContext joinableTaskContext)
        {
            return new ToolkitThreadHelper(joinableTaskContext);
        }

        /// <summary>
        /// Private constructor; call <see cref="Create"/> or <see cref="CreateWithContext"/>.
        /// </summary>
        /// <param name="joinableTaskContext">The application's one-and-only <see cref="JoinableTaskContext"/>.</param>
        private ToolkitThreadHelper(JoinableTaskContext joinableTaskContext)
        {
            _context = joinableTaskContext;
            _collection = joinableTaskContext.CreateCollection();
            JoinableTaskFactory = joinableTaskContext.CreateFactory(_collection);
        }

        /// <summary>
        /// The JoinableTaskFactory instance.
        /// </summary>
        public JoinableTaskFactory JoinableTaskFactory { get; }

        /// <summary>
        /// Gets a <see cref="CancellationToken"/> that can be used to check if the class has been disposed.
        /// </summary>
        public CancellationToken DisposalToken => _disposeCancellationTokenSource.Token;

        /// <summary>
        /// Joins any unfinished tasks.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Joins any unfinished tasks.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposeCancellationTokenSource.Cancel();

                try
                {
                    // Block Dispose until all async work has completed.
                    _context.Factory.Run(_collection.JoinTillEmptyAsync);
                }
                catch (OperationCanceledException)
                {
                    // this exception is expected because we signaled the cancellation token
                }
                catch (AggregateException ex)
                {
                    // ignore AggregateException containing only OperationCanceledException
                    ex.Handle(inner => (inner is OperationCanceledException));
                }
                finally
                {
                    _disposeCancellationTokenSource.Dispose();
                    _disposed = true;
                }
            }
        }
    }
}
