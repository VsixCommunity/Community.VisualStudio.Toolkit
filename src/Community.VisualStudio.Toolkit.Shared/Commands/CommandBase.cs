using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class that makes it easier to handle commands.
    /// </summary>
    /// <example>
    /// <code>
    /// public class TestCommand : BaseCommand&lt;TestCommand&gt;
    /// {
    ///     public TestCommand() : base(new Guid("489ba882-f600-4c8b-89db-eb366a4ee3b3"), 0x000)
    ///     { }
    /// 
    ///     protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
    ///     {
    ///         return base.ExecuteAsync(e);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="T">The implementation type itself.</typeparam>
    public abstract class BaseCommand<T> where T : BaseCommand<T>, new()
    {
        private CommandID _commandId { get; }

        /// <summary>
        /// Creates a new instance of the implementation.
        /// </summary>
        protected BaseCommand(Guid commandGuid, int commandId)
        {
            _commandId = new CommandID(commandGuid, commandId);
        }

        /// <summary>
        /// The command object associated with the command ID (guid/id).
        /// </summary>
        public OleMenuCommand? Command { get; private set; }

        /// <summary>
        /// The package class that initialized this class.
        /// </summary>
        public AsyncPackage? Package { get; private set; }

        /// <summary>
        /// Initializes the command. This method must be called from the <see cref="AsyncPackage.InitializeAsync"/> method for the command to work.
        /// </summary>
        public static async Task<T> InitializeAsync(AsyncPackage package)
        {
            var instance = new T();

            instance.Command = new OleMenuCommand(instance.Execute, instance._commandId);
            instance.Package = package;

            instance.Command.BeforeQueryStatus += (s, e) => { instance.BeforeQueryStatus(e); };

            var commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));
            Assumes.Present(commandService);

            commandService?.AddCommand(instance.Command);

            await instance.InitializeCompletedAsync();
            return instance;
        }

        /// <summary>Allows the implementor to manipulate the command before its execution.</summary>
        /// <remarks>
        /// This method is invoked right after the <see cref="InitializeAsync(AsyncPackage)"/> method is executed and allows you to
        /// manipulate the <see cref="Command"/> property etc. as part of the initialization phase.
        /// </remarks>
        protected virtual Task InitializeCompletedAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>Executes synchronously when the command is invoked.</summary>
        /// <remarks>
        /// Use this method instead of <see cref="ExecuteAsync"/> if you're not performing any async tasks using async/await patterns.
        /// </remarks>
        protected virtual void Execute(object sender, EventArgs e)
        {
            Package?.JoinableTaskFactory.RunAsync(async delegate
            {
                try
                {
                    await ExecuteAsync((OleMenuCmdEventArgs)e);
                }
                catch (Exception ex)
                {
                    await ex.LogAsync();
                }
            });
        }

        /// <summary>Executes asynchronously when the command is invoked and <c>Execute(object, EventArgs)</c> isn't overridden.</summary>
        /// <remarks>Use this method instead of <see cref="Execute"/> if you're invoking any async tasks by using async/await patterns.</remarks>
        protected virtual Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return Task.CompletedTask;
        }

        /// <summary>Override this method to control the commands visibility and other properties.</summary>
        protected virtual void BeforeQueryStatus(EventArgs e)
        {
            // Leave empty
        }
    }
}