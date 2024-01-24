using System;
using System.Collections.Generic;
using System.Reflection;

namespace dk.UWP.Extension {
    /// <summary>
    /// Extensions for TypeInfo.
    /// Based on code from http://stackoverflow.com/questions/5737840/whats-the-difference-between-system-type-and-system-runtimetype-in-c.
    /// </summary>
    public static class TypeInfoExtensions {
        #if NETFX_CORE

        /// <summary>
        /// Provides an enumeration of all constructors for a TypeInfo instance.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns>An enumeration of all constructors for a TypeInfo instance.</returns>
        public static IEnumerable<ConstructorInfo> GetAllConstructors(this TypeInfo typeInfo)
            => GetAll(typeInfo, ti => ti.DeclaredConstructors);

        /// <summary>
        /// Provides an enumeration of all events for a TypeInfo instance.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns>An enumeration of all events for a TypeInfo instance.</returns>
        public static IEnumerable<EventInfo> GetAllEvents(this TypeInfo typeInfo)
            => GetAll(typeInfo, ti => ti.DeclaredEvents);

        /// <summary>
        /// Provides an enumeration of all fields for a TypeInfo instance.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns>An enumeration of all fields for a TypeInfo instance.</returns>
        public static IEnumerable<FieldInfo> GetAllFields(this TypeInfo typeInfo)
            => GetAll(typeInfo, ti => ti.DeclaredFields);

        /// <summary>
        /// Provides an enumeration of all members for a TypeInfo instance.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns>An enumeration of all members for a TypeInfo instance.</returns>
        public static IEnumerable<MemberInfo> GetAllMembers(this TypeInfo typeInfo)
            => GetAll(typeInfo, ti => ti.DeclaredMembers);

        /// <summary>
        /// Provides an enumeration of all methods for a TypeInfo instance.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns>An enumeration of all methods for a TypeInfo instance.</returns>
        public static IEnumerable<MethodInfo> GetAllMethods(this TypeInfo typeInfo)
            => GetAll(typeInfo, ti => ti.DeclaredMethods);

        /// <summary>
        /// Provides an enumeration of all nested types for a TypeInfo instance.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns>An enumeration of all nested types for a TypeInfo instance.</returns>
        public static IEnumerable<TypeInfo> GetAllNestedTypes(this TypeInfo typeInfo)
            => GetAll(typeInfo, ti => ti.DeclaredNestedTypes);

        /// <summary>
        /// Provides an enumeration of all properties for a TypeInfo instance.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns>An enumeration of all properties for a TypeInfo instance.</returns>
        public static IEnumerable<PropertyInfo> GetAllProperties(this TypeInfo typeInfo)
            => GetAll(typeInfo, ti => ti.DeclaredProperties);

        /// <summary>
        /// Gets an enumerations of all accessors of the specified type for a given TypeInfo.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeInfo"></param>
        /// <param name="accessor"></param>
        /// <returns>An enumerations of all accessors of the specified type for a given TypeInfo.</returns>
        private static IEnumerable<T> GetAll<T>(TypeInfo typeInfo, Func<TypeInfo, IEnumerable<T>> accessor) {
            while (typeInfo != null) {
                foreach (var t in accessor(typeInfo)) {
                    yield return t;
                }

                typeInfo = typeInfo.BaseType?.GetTypeInfo();
            }
        }

        #endif
    }
}
