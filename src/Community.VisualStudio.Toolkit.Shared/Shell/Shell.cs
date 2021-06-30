using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>A collection of services related to the command system.</summary>
    public class Shell
    {
        internal Shell()
        { }

        /// <summary>
        /// Gets the version of Visual Studio.
        /// </summary>
        public async Task<Version?> GetVsVersionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsShell? shell = await VS.Services.GetShellAsync();

            shell.GetProperty((int)__VSSPROPID5.VSSPROPID_ReleaseVersion, out var value);

            if (value is string raw)
            {
                return Version.Parse(raw.Split(' ')[0]);
            }

            return null;
        }

        /// <summary>
        /// Get the value passed in to the specified command line argument key.
        /// </summary>
        public async Task<string> TryGetCommandLineArgumentAsync(string key)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsAppCommandLine? acl = await VS.Services.GetAppCommandLineAsync();
            acl.GetOption(key, out _, out var value);

            return value;
        }

        /// <summary>
        /// Restarts the IDE. 
        /// </summary>
        /// <param name="forceElevated">Forces the IDE to start back up elevated. 
        /// If <see langword="false"/>, it restarts in the same mode it is currently running in.
        /// </param>
        public async Task RestartAsync(bool forceElevated = false)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var shell = (IVsShell4)await VS.Services.GetShellAsync();

            if (forceElevated)
            {
                shell.Restart((uint)__VSRESTARTTYPE.RESTART_Elevated);
            }
            else
            {
                ((IVsShell3)shell).IsRunningElevated(out var elevated);
                __VSRESTARTTYPE type = elevated ? __VSRESTARTTYPE.RESTART_Elevated : __VSRESTARTTYPE.RESTART_Normal;
                shell.Restart((uint)type);
            }
        }
    }
}
