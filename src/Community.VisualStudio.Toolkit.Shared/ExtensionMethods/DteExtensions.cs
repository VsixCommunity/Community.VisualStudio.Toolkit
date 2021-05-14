using System;
using Microsoft.VisualStudio.Shell;

namespace EnvDTE
{
    /// <summary>Extension methods for the DTE.</summary>
    public static class DteExtensions
    {
        /// <summary>
        /// Builds the solution asynchronously
        /// </summary>
        /// <returns>Returns 'true' if successful</returns>
        public static bool TryExecuteCommand(this _DTE dte, string commandName, string commandArgs = "")
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
    }
}
