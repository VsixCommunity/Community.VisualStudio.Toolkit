using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Extensions for an <see cref="AsyncPackage"/>
    /// </summary>
    public static class AsyncPackageExtensions
    {
        /// <summary>
        /// Automatically calls the <see cref="BaseCommand{T}.InitializeAsync(AsyncPackage)"/> method for every command that has the <see cref="CommandAttribute"/> applied.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="assemblies"></param>
        /// <returns>A collection of the command instances</returns>
        public static async Task<IEnumerable<object>> RegisterCommandsAsync(this AsyncPackage package, params Assembly[] assemblies)
        {
            Type baseCommandType = typeof(BaseCommand<>);
            IEnumerable<Type> commandTypes = IncludePackageAssembly(assemblies, package).SelectMany(x => x.GetTypes())
                .Where(x =>
                    !x.IsAbstract
                    && x.IsAssignableToGenericType(baseCommandType)
                    && x.GetCustomAttribute<CommandAttribute>() != null);

            List<object> commands = new();
            foreach (Type commandType in commandTypes)
            {
                MethodInfo initializeAsyncMethod = commandType.GetMethod(
                    nameof(BaseCommand<object>.InitializeAsync),
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                Task initializeAsyncTask = (Task)initializeAsyncMethod.Invoke(null, new object[] { package });
                await initializeAsyncTask.ConfigureAwait(false);
                object command = initializeAsyncTask.GetType().GetProperty("Result").GetValue(initializeAsyncTask);
                commands.Add(command);
            }
            return commands;
        }

        /// <summary>
        /// Automatically calls the <see cref="BaseCommand{T}.InitializeAsync(AsyncPackage)"/> method for every command that has the <see cref="CommandAttribute"/> applied.
        /// </summary>
        /// <param name="package"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static void RegisterToolWindows(this AsyncPackage package, params Assembly[] assemblies)
        {
            Type baseToolWindowType = typeof(BaseToolWindow<>);
            IEnumerable<Type> toolWindowTypes = IncludePackageAssembly(assemblies, package).SelectMany(x => x.GetTypes())
                .Where(x =>
                    !x.IsAbstract
                    && x.IsAssignableToGenericType(baseToolWindowType));

            foreach (Type toolWindowtype in toolWindowTypes)
            {
                MethodInfo initializeMethod = toolWindowtype.GetMethod(
                    "Initialize",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                initializeMethod.Invoke(null, new object[] { package });
            }
        }

        /// <summary>
        /// Finds, creates and registers <see cref="BaseFontAndColorProvider"/> implementations.
        /// </summary>
        /// <param name="package">The package to register the provider in.</param>
        /// <param name="assemblies">Additional assemblies to scan. The assembly that <paramref name="package"/> is in will always be scanned.</param>
        /// <returns>A task.</returns>
        public static async Task RegisterFontAndColorProvidersAsync(this AsyncPackage package, params Assembly[] assemblies)
        {
            Type baseProviderType = typeof(BaseFontAndColorProvider);
            IEnumerable<Type> providerTypes = IncludePackageAssembly(assemblies, package)
                .SelectMany(x => x.GetTypes())
                .Where(x => !x.IsAbstract && baseProviderType.IsAssignableFrom(x));

            foreach (Type providerType in providerTypes)
            {
                ConstructorInfo? ctor = providerType.GetConstructor(Type.EmptyTypes)
                    ?? throw new InvalidOperationException($"The type '{providerType.Name}' must have a parameterless constructor.");
                BaseFontAndColorProvider provider = (BaseFontAndColorProvider)ctor.Invoke(Array.Empty<object>());
                await provider.InitializeAsync(package);
            }
        }

        private static IReadOnlyList<Assembly> IncludePackageAssembly(IEnumerable<Assembly> assemblies, AsyncPackage package)
        {
            List<Assembly> list = assemblies.ToList();
            Assembly packageAssembly = package.GetType().Assembly;
            if (!list.Contains(packageAssembly))
            {
                list.Add(packageAssembly);
            }
            return list;
        }
    }
}
