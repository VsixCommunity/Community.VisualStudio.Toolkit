using System;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// The entry point for a wide variety of extensibility helper classes and methods.
    /// </summary>
    public static class VS
    {
        private static Build? _build;
        /// <summary>Handles building of solutions and projects.</summary>
        public static Build Build => _build ??= new();

        private static Commands? _commands;
        /// <summary>A collection of services related to the command system.</summary>
        public static Commands Commands => _commands ??= new();

        private static Documents? _documents;
        /// <summary>Contains helper methods for dealing with documents.</summary>
        public static Documents Documents => _documents ??= new();

        private static Events? _events;
        /// <summary>A collection of events.</summary>
        public static Events Events => _events ??= new();

        private static InfoBarFactory? _infoBarFactory;
        /// <summary>Creates InfoBar controls for use on documents and tool windows.</summary>
        public static InfoBarFactory InfoBar => _infoBarFactory ??= new();

        private static MessageBox? _messageBox;
        /// <summary>Shows message boxes.</summary>
        public static MessageBox MessageBox => _messageBox ??= new();

        private static Selection? _selection;
        /// <summary>Services related to the selection of windows and items in solution.</summary>
        public static Selection Selection => _selection ??= new();

        private static Services? _services;
        /// <summary>A collection of services commonly used by extensions.</summary>
        public static Services Services => _services ??= new();

        private static Settings? _settings;
        /// <summary>A collection of services related to settings.</summary>
        public static Settings Settings => _settings ??= new();

        private static Shell? _shell;
        /// <summary>A collection of services related to the shell.</summary>
        public static Shell Shell => _shell ??= new();

        private static Solutions? _solutions;
        /// <summary>A collection of services related to solutions.</summary>
        public static Solutions Solutions => _solutions ??= new();

        private static StatusBar? _statusBar;
        /// <summary>An API wrapper that makes it easy to work with the status bar.</summary>
        public static StatusBar StatusBar => _statusBar ??= new();

        private static Windows? _windows;
        /// <summary>A collection of services related to windows.</summary>
        public static Windows Windows => _windows ??= new();

        /// <summary>
        /// Gets a global service asynchronously.
        /// </summary>
        /// <typeparam name="TService">The type identity of the service.</typeparam>
        /// <typeparam name="TInterface">The interface to cast the service to.</typeparam>
        /// <returns>A task whose result is the service, if found; otherwise <see langword="null" />.</returns>
        public static async Task<TInterface?> GetServiceAsync<TService, TInterface>() where TService : class where TInterface : class
        {
#if VS14
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return (TInterface)await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(TService));
#elif VS15
            return await ServiceProvider.GetGlobalServiceAsync<TService, TInterface>();
#else
            return await ServiceProvider.GetGlobalServiceAsync<TService, TInterface>(swallowExceptions: false);
#endif
        }

        /// <summary>
        /// Gets a global service asynchronously.
        /// </summary>
        /// <typeparam name="TService">The type identity of the service.</typeparam>
        /// <typeparam name="TInterface">The interface to cast the service to.</typeparam>
        /// <returns>A task whose result is the service, if found.</returns>
        /// <exception cref="Exception">Throws an exception when the service is not available.</exception>
        public static async Task<TInterface> GetRequiredServiceAsync<TService, TInterface>() where TService : class where TInterface : class
        {
            TInterface? service = await GetServiceAsync<TService, TInterface>();
            Assumes.Present(service);

            return service!;
        }

        /// <summary>
        /// Gets a global service synchronously.
        /// </summary>
        /// <typeparam name="TService">The type identity of the service.</typeparam>
        /// <typeparam name="TInterface">The interface to cast the service to.</typeparam>
        /// <exception cref="Exception">Throws an exception when the service is not available.</exception>
        public static TInterface GetRequiredService<TService, TInterface>() where TService : class where TInterface : class
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return (TInterface)ServiceProvider.GlobalProvider.GetService(typeof(TService));
        }

        /// <summary>
        /// Gets a service from the MEF component catalog
        /// </summary>
        public static async Task<TInterface> GetMefServiceAsync<TInterface>() where TInterface : class
        {
            IComponentModel2 compService = await GetRequiredServiceAsync<SComponentModel, IComponentModel2>();
            return compService.GetService<TInterface>();
        }

        /// <summary>
        /// Gets a service from the MEF component catalog
        /// </summary>
        public static TInterface GetMefService<TInterface>() where TInterface : class
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IComponentModel2 compService = GetRequiredService<SComponentModel, IComponentModel2>();
            return compService.GetService<TInterface>();
        }

        /// <summary>
        /// Restarts Visual Studio.
        /// </summary>
        public static async Task<bool> RestartAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsShell4 shell = await GetRequiredServiceAsync<SVsShell, IVsShell4>();

            ((IVsShell3)shell).IsRunningElevated(out bool elevated);
            __VSRESTARTTYPE type = elevated ? __VSRESTARTTYPE.RESTART_Elevated : __VSRESTARTTYPE.RESTART_Normal;
            int hr = shell.Restart((uint)type);

            return ErrorHandler.Succeeded(hr);
        }
    }
}
