using System;
using System.ComponentModel.Design;
using System.Linq;
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
    /// [Command("489ba882-f600-4c8b-89db-eb366a4ee3b3", 0x0100)]
    /// public class TestCommand : BaseCommand&lt;TestCommand&gt;
    /// {
    ///     protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
    ///     {
    ///         return base.ExecuteAsync(e);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="T">The implementation type itself.</typeparam>
    public abstract class BaseCommand<T> where T : class, new()
    {
        /// <summary>
        /// The command object associated with the command ID (GUID/ID).
        /// </summary>
        public OleMenuCommand Command { get; protected set; } = null!; // This property is initialized in `InitializeAsync`, so it's never actually null.

        /// <summary>
        /// The package class that initialized this class.
        /// </summary>
        public AsyncPackage Package { get; protected set; } = null!; // This property is initialized in `InitializeAsync`, so it's never actually null.

        /// <summary>
        /// Initializes the command. This method must be called from the <see cref="AsyncPackage.InitializeAsync"/> method for the command to work.
        /// </summary>
        public static async Task<T> InitializeAsync(AsyncPackage package)
        {
            BaseCommand<T> instance = (BaseCommand<T>)(object)new T();

            CommandAttribute? attr = (CommandAttribute)instance.GetType().GetCustomAttributes(typeof(CommandAttribute), true).FirstOrDefault();

            if (attr is null)
            {
                throw new InvalidOperationException($"No [Command(GUID, ID)] attribute was added to {typeof(T).Name}");
            }

            // Use package GUID if no command set GUID has been specified
            Guid cmdGuid = attr.Guid == Guid.Empty ? package.GetType().GUID : attr.Guid;
            CommandID cmd = new(cmdGuid, attr.Id);

            if (instance is IBaseDynamicCommand dynamicCommand)
            {
                instance.Command = new DynamicItemMenuCommand(dynamicCommand.IsMatch, instance.Execute, instance.BeforeQueryStatus, cmd);
            }
            else
            {
                instance.Command = new OleMenuCommand(instance.Execute, changeHandler: null, instance.BeforeQueryStatus, cmd);
            }

            instance.Package = package;


            await package.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            IMenuCommandService commandService = (IMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));
            Assumes.Present(commandService);

            commandService.AddCommand(instance.Command);  // Requires main/UI thread

            await instance.InitializeCompletedAsync();
            return (T)(object)instance;
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
            Package.JoinableTaskFactory.RunAsync(async delegate
            {
                try
                {
                    await ExecuteAsync((OleMenuCmdEventArgs)e);
                }
                catch (Exception ex)
                {
                    await ex.LogAsync();
                }
            }).FireAndForget();
        }

        /// <summary>Executes asynchronously when the command is invoked and <see cref="Execute(object, EventArgs)"/> isn't overridden.</summary>
        /// <remarks>Use this method instead of <see cref="Execute"/> if you're invoking any async tasks by using async/await patterns.</remarks>
        protected virtual Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return Task.CompletedTask;
        }

        internal virtual void BeforeQueryStatus(object sender, EventArgs e)
        {
            BeforeQueryStatus(e);
        }

        /// <summary>Override this method to control the commands visibility and other properties.</summary>
        protected virtual void BeforeQueryStatus(EventArgs e)
        {
            // Leave empty
        }
    }
}