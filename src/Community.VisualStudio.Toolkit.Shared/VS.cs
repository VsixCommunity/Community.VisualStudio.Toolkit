using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// The entry point for a wide variety of extensibility helper classes and methods.
    /// </summary>
    public static class VS
    {
        /// <summary>A collection of services related to the command system.</summary>
        public static Commanding Commanding => new();

        /// <summary>A collection of services related to the debugger.</summary>
        public static Debugger Debugger => new();

        /// <summary>A collection of services related to the editor.</summary>
        public static Editor Editor => new();

        /// <summary>A collection of events.</summary>
        public static Events Events => new();

        /// <summary>A collection of services related to notifying the users.</summary>
        public static Notifications Notifications => new();

        /// <summary>A collection of services related to the shell.</summary>
        public static Shell Shell => new();

        /// <summary>A collection of services related to solutions.</summary>
        public static Solution Solution => new();

        /// <summary>A collection of services related to windows.</summary>
        public static Windows Windows => new();

        /// <summary>Get the EnvDTE which provide a broad API for a large part of Visual Studio.</summary>
        public static Task<DTE2> GetDTEAsync() => GetServiceAsync<DTE, DTE2>();

        /// <summary>
        /// Gets a global service asynchronously.
        /// </summary>
        /// <typeparam name="TService">The type identity of the service.</typeparam>
        /// <typeparam name="TInterface">The interface to cast the service to.</typeparam>
        /// <returns>A task who's result is the service, if found; otherwise <see langword="">null</see>.</returns>
        public static async Task<TInterface> GetServiceAsync<TService, TInterface>() where TService : class where TInterface : class
        {
#if VS14
            var service = (TInterface)await AsyncServiceProvider.GlobalProvider.GetServiceAsync(typeof(TService));
#elif VS15
            TInterface? service = await ServiceProvider.GetGlobalServiceAsync<TService, TInterface>();
#else
            TInterface? service = await ServiceProvider.GetGlobalServiceAsync<TService, TInterface>(swallowExceptions: false);
#endif
            Assumes.Present(service);
            return service;
        }
    }
}
