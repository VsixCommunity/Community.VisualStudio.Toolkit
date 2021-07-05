using System;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// The entry point for a wide variety of extensibility helper classes and methods.
    /// </summary>
    public static class VS
    {
        /// <summary>Handles building of solutions and projects.</summary>
        public static Build Build { get; } = new();

        /// <summary>A collection of services related to the command system.</summary>
        public static Commands Commands { get; } = new();

        /// <summary>Contains helper methods for dealing with documents.</summary>
        public static Documents Documents { get; } = new();

        /// <summary>A collection of events.</summary>
        public static Events Events { get; } = new();

        /// <summary>Creates InfoBar controls for use on documents and tool windows.</summary>
        public static InfoBarFactory InfoBar { get; } = new();

        /// <summary>Shows message boxes.</summary>
        public static MessageBox MessageBox { get; } = new();

        /// <summary>Services related to the selection of windows and items in solution.</summary>
        public static Selection Selection { get; } = new();

        /// <summary>A collection of services commonly used by extensions.</summary>
        public static Services Services { get; } = new();

        /// <summary>A collection of services related to settings.</summary>
        public static Settings Settings { get; } = new();

        /// <summary>A collection of services related to the shell.</summary>
        public static Shell Shell { get; } = new();

        /// <summary>A collection of services related to solutions.</summary>
        public static Solution Solution { get; } = new();

        /// <summary>An API wrapper that makes it easy to work with the status bar.</summary>
        public static StatusBar StatusBar { get; } = new();

        /// <summary>A collection of services related to windows.</summary>
        public static Windows Windows { get; } = new();

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
            IComponentModel2 compService = (IComponentModel2)ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel));
            Assumes.Present(compService);
            return compService.GetService<TInterface>();
        }
    }
}
