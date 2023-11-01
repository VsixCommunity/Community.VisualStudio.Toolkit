using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Shell;

namespace Community.VisualStudio.Toolkit
{
    /// <summary>
    /// Extensions for an <see cref="ToolkitPackage"/>
    /// </summary>
    public static class ToolkitPackageExtensions
    {
        /// <summary>
        /// Automatically calls the <see cref="BaseToolWindow{T}.Initialize(ToolkitPackage)"/> 
        /// method for every <see cref="BaseToolWindow{T}"/> in the package or provided assemblies.
        /// </summary>
        /// <param name="package">The package that contains the tool windows to register.</param>
        /// <param name="assemblies">The additional assemblies to look for tool windows in.</param>
        public static void RegisterToolWindows(this ToolkitPackage package, params Assembly[] assemblies)
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
