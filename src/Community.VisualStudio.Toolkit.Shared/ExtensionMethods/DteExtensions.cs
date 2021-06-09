using System;
using Community.VisualStudio.Toolkit;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace EnvDTE80
{
    /// <summary>Extension methods for the DTE.</summary>
    public static class DteExtensions
    {
        /// <summary>
        /// Executes a command safely without throwing exceptions.
        /// </summary>
        /// <returns>Returns 'true' if successful</returns>
        public static bool TryExecuteCommand(this DTE2 dte, string commandName, string commandArgs = "")
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                Command? cmd = dte.Commands.Item(commandName);

                if (cmd != null && cmd.IsAvailable)
                {
                    dte.ExecuteCommand(commandName, commandArgs);
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }

            return false;
        }

        /// <summary>
        /// Creates an <c>IDisposable</c> undo context to wrap in a using statement.
        /// </summary>
        /// <param name="dte">The DTE instance.</param>
        /// <param name="name">The name to appear as the undo item.</param>
        public static IDisposable CreateUndoContext(this DTE2 dte, string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            dte.UndoContext.Open(name);

            return new Disposable(dte.UndoContext.Close);
        }
    }
}
