using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
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

        /// <summary>Provides access to the fundamental environment services, specifically those dealing with VSPackages and the registry.</summary>
        public Task<IVsShell> GetShellAsync() => VS.GetRequiredServiceAsync<SVsShell, IVsShell>();

        /// <summary>This interface provides access to basic windowing functionality, including access to and creation of tool windows and document windows.</summary>
        public Task<IVsUIShell> GetUIShellAsync() => VS.GetRequiredServiceAsync<SVsUIShell, IVsUIShell>();

        /// <summary>This interface is used by a package to read command-line switches entered by the user.</summary>
        public Task<IVsAppCommandLine> GetAppCommandLineAsync() => VS.GetRequiredServiceAsync<SVsAppCommandLine, IVsAppCommandLine>();

        /// <summary>Registers well-known images (such as icons) for Visual Studio.</summary>
        /// <returns>Cast return object to <see cref="IVsImageService2"/></returns>
        public Task<object> GetImageServiceAsync() => VS.GetRequiredServiceAsync<SVsImageService, object>();

        /// <summary>Controls the caching of font and color settings.</summary>
        public Task<IVsFontAndColorCacheManager> GetFontAndColorCacheManagerAsync() => VS.GetRequiredServiceAsync<SVsFontAndColorCacheManager, IVsFontAndColorCacheManager>();

        /// <summary>Allows a VSPackage to retrieve or save font and color data to the registry.</summary>
        public Task<IVsFontAndColorStorage> GetFontAndColorStorageAsync() => VS.GetRequiredServiceAsync<SVsFontAndColorStorage, IVsFontAndColorStorage>();

        /// <summary>Controls the most recently used (MRU) items collection.</summary>
        /// <returns>Cast return object to <see cref="IVsMRUItemsStore"/></returns>
        public Task<object> GetMRUItemsStoreAsync() => VS.GetRequiredServiceAsync<SVsMRUItemsStore, object>();

        /// <summary>Used to retrieved services defined in the MEF catalog, such as the editor specific services like <see cref="IVsEditorAdaptersFactoryService"/>.</summary>
        public Task<IComponentModel2> GetComponentModelAsync() => VS.GetRequiredServiceAsync<SComponentModel, IComponentModel2>();

        /// <summary>
        /// Gets the version of Visual Studio.
        /// </summary>
        public async Task<Version?> GetVsVersionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsShell? shell = await GetShellAsync();

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

            IVsAppCommandLine? acl = await GetAppCommandLineAsync();
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
            var shell = (IVsShell4)await GetShellAsync();

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
