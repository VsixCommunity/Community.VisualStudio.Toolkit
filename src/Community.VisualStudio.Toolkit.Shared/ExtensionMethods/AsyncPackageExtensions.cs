using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Community.VisualStudio.Toolkit.Shared.ExtensionMethods
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
            List<Assembly> assembliesList = assemblies.ToList();
            Assembly packageAssembly = package.GetType().Assembly;
            if (!assembliesList.Contains(packageAssembly))
                assembliesList.Add(packageAssembly);

            Type baseCommandType = typeof(BaseCommand<>);
            IEnumerable<Type> commandTypes = assembliesList.SelectMany(x => x.GetTypes())
                .Where(x =>
                    !x.IsAbstract
                    && x.IsAssignableToGenericType(baseCommandType)
                    && x.GetCustomAttribute<CommandAttribute>() != null);

            List<object>? commands = new List<object>();
            foreach (Type? commandType in commandTypes)
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
            List<Assembly> assembliesList = assemblies.ToList();
            Assembly packageAssembly = package.GetType().Assembly;
            if (!assembliesList.Contains(packageAssembly))
                assembliesList.Add(packageAssembly);

            Type baseToolWindowType = typeof(BaseToolWindow<>);
            IEnumerable<Type> toolWindowTypes = assembliesList.SelectMany(x => x.GetTypes())
                .Where(x =>
                    !x.IsAbstract
                    && x.IsAssignableToGenericType(baseToolWindowType));

            foreach (Type? toolWindowtype in toolWindowTypes)
            {
                MethodInfo initializeMethod = toolWindowtype.GetMethod(
                    "Initialize",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                initializeMethod.Invoke(null, new object[] { package });
            }
        }
    }
}
