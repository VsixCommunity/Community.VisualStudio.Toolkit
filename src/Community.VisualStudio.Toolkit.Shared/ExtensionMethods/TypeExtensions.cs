using System;
using System.Linq;

namespace Community.VisualStudio.Toolkit.Shared.ExtensionMethods
{
    /// <summary>
    /// Extensions for <see cref="Type"/>
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines if a type is assignable (inherits) from the specified generic type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        public static bool IsAssignableToGenericType(this Type type, Type genericType)
        {
            return type.FindGenericBaseType(genericType) != null;
        }

        /// <summary>
        /// Attempts to find the specified generic type in the inheritance hierarchy for the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericType"></param>
        /// <returns>The generic type if present, otherwise null.</returns>
        public static Type? FindGenericBaseType(this Type type, Type genericType)
        {
            if (type == null || genericType == null)
                return null;

            if (type == genericType)
                return type;

            // See if any of the base types implement the type
            while (type != null && type != typeof(object))
            {
                Type? currentType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (genericType == currentType)
                    return currentType;

                // See if any of the interfaces implement the type
                Type? interfaceType = type.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericType);

                if (interfaceType != null)
                    return interfaceType;

                type = type.BaseType;
            }
            return null;
        }
    }
}
