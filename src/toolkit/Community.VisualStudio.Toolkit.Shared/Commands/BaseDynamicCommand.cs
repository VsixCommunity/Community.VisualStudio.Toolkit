using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// A base class that makes it easier to handle dynamic menu commands.
    /// </summary>
    /// <example>
    /// <code>
    /// [Command("a4477648-9ba7-4bbc-af04-8b7e931f88ab", 0x0100)]
    /// public class TestCommand : BaseDynamicCommand&lt;TestCommand, MyObject&gt;
    /// {
    ///     protected async override Task ExecuteAsync(OleMenuCmdEventArgs e, MyObject item)
    ///     {
    ///         await item.DoSomethingAsync();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="TCommand">The implementation type itself.</typeparam>
    /// <typeparam name="TItem">The type of item that a menu item is dynamically created for.</typeparam>
    public abstract class BaseDynamicCommand<TCommand, TItem> : BaseCommand<TCommand>, IBaseDynamicCommand where TCommand : class, new()
    {
        private static readonly object _dynamicMenuItemKey = new();

        private IReadOnlyList<TItem>? _items;

        /// <summary>
        /// Gets the items that the menu items will be created for. One menu item will be created for each item that is returned.
        /// </summary>
        protected abstract IReadOnlyList<TItem> GetItems();

        bool IBaseDynamicCommand.IsMatch(int commandId)
        {
            int index = commandId - Command.CommandID.ID;
            return TryGetItem(index, out _);
        }

        /// <inheritdoc/>
        internal sealed override void BeforeQueryStatus(object sender, EventArgs e)
        {
            DynamicItemMenuCommand menuItem = (DynamicItemMenuCommand)sender;
            int index;

            if (Command.MatchedCommandId == 0)
            {
                // The matched command ID will be zero for the base item. The base item is always
                // called first, then `IsMatch` is called repeatedly with increasing command IDs
                // until it returns false. In `IsMatch`, we return use the count of items to
                // determine when to stop, so this is the perfect opportunity to get the latest
                // items so that the dynamic menu items reflect the latest set of items.
                index = 0;
                _items = GetItems();
            }
            else
            {
                // For all other items, the matched command ID
                // will be the base command ID plus the index.
                index = Command.MatchedCommandId - Command.CommandID.ID;
            }

            // There may be no items, but we'll always be called for the base 
            // item, which means the index could be out of bounds. If the index
            // is out of bounds, then we'll hide and disable the menu item.
            if (TryGetItem(index, out TItem item))
            {
                // If there are no items, then each dynamic menu item will be disabled and
                // hidden in the `else` case below. We don't want to rely on `BeforeQueryStatus`
                // having to make the menu items enabled and visible, so we will make them enabled
                // and visible by default, and `BeforeQueryStatus` can change that if it needs to.
                menuItem.Enabled = true;
                menuItem.Visible = true;

                BeforeQueryStatus(menuItem, e, item);

                // Store the index in the menu item's properties so that we can
                // determine the index of the menu item when it is executed.
                menuItem.Properties[_dynamicMenuItemKey] = index;
            }
            else
            {
                menuItem.Enabled = false;
                menuItem.Visible = false;
                menuItem.Properties.Remove(_dynamicMenuItemKey);
            }

            // We have finished handling this item, so clear
            // the matched command ID on the base command.
            Command.MatchedCommandId = 0;
        }

        /// <summary>Override this method to control the commands visibility and other properties.</summary>
        protected abstract void BeforeQueryStatus(OleMenuCommand menuItem, EventArgs e, TItem item);

        /// <inheritdoc/>
        protected override sealed void Execute(object sender, EventArgs e)
        {
            DynamicItemMenuCommand menuItem = (DynamicItemMenuCommand)sender;
            if (menuItem.Properties.Contains(_dynamicMenuItemKey))
            {
                int index = (int)menuItem.Properties[_dynamicMenuItemKey];
                if (TryGetItem(index, out TItem item))
                {
                    Execute((OleMenuCmdEventArgs)e, item);
                }
            }
        }

        /// <summary>Executes synchronously when the command is invoked.</summary>
        /// <remarks>
        /// Use this method instead of <see cref="ExecuteAsync(OleMenuCmdEventArgs, TItem)"/> if you're not performing any async tasks using async/await patterns.
        /// </remarks>
        protected virtual void Execute(OleMenuCmdEventArgs e, TItem item)
        {
            Package.JoinableTaskFactory.RunAsync(async delegate
            {
                try
                {
                    await ExecuteAsync(e, item);
                }
                catch (Exception ex)
                {
                    await ex.LogAsync();
                }
            }).FireAndForget();
        }

        /// <summary>Executes asynchronously when the command is invoked and <see cref="Execute(object, EventArgs)"/> isn't overridden.</summary>
        /// <remarks>Use this method instead of <see cref="Execute(OleMenuCmdEventArgs, TItem)"/> if you're invoking any async tasks by using async/await patterns.</remarks>
        protected virtual Task ExecuteAsync(OleMenuCmdEventArgs e, TItem item)
        {
            return Task.CompletedTask;
        }

        private bool TryGetItem(int index, out TItem item)
        {
            if (_items is null)
            {
                item = default!;
                return false;
            }

            if ((index >= 0) && (index < _items.Count))
            {
                item = _items[index];
                return true;
            }

            item = default!;
            return false;
        }

        /// <inheritdoc/>
        protected override sealed void BeforeQueryStatus(EventArgs e)
        {
            // This method will not be called for dynamic commands. This is
            // sealed to prevent consumers from unnecessarily overriding it.
        }

        /// <inheritdoc/>
        protected override sealed Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // This method will not be called for dynamic commands. This is
            // sealed to prevent consumers from unnecessarily overriding it.
            return Task.CompletedTask;
        }
    }

    internal interface IBaseDynamicCommand
    {
        bool IsMatch(int commandId);
    }
}
