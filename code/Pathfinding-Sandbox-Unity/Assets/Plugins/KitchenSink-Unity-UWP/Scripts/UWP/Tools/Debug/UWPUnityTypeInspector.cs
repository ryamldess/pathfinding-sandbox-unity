using dk.Tools.Debug;
using dk.UWP.Extension;
using dk.UWP.Interfaces;

using System;
using System.Collections.Generic;
using System.Reflection;

#if NETFX_CORE

using System.Linq;
using System.Threading.Tasks;

#endif

namespace dk.UWP.Tools.Debug {
    /// <summary>
    /// UWP implementation of UnityTypeInspector.
    /// </summary>
    public class UWPUnityTypeInspector : IUWPUnityTypeInspectorWrapper {
        #if NETFX_CORE

        /// <summary>
        /// Gets the inheritance chain as a list of TypeInfo objects.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="list"></param>
        public static void GetInheritanceChain(Type type, List<TypeInfo> list) {
            List<Type> typeList = new List<Type>();
            UnityTypeInspector.GetInheritanceChain(type, typeList);

            foreach (Type t in typeList) {
                list.Add(t.GetTypeInfo());
            }
        }

        /// <summary>
        /// Gets the parent types as a list of TypeInfo objects.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="list">List.</param>
        public static void GetParentTypes(Type type, List<TypeInfo> list) {
            List<Type> typeList = new List<Type>();
            UnityTypeInspector.GetParentTypes(type, typeList);

            foreach (Type t in typeList) {
                list.Add(t.GetTypeInfo());
            }
        }

        #endif

        #region IUWPUnityTypeInspectorWrapper implementations

            /// <summary>
            /// Gets the inheritance chain.
            /// </summary>
            /// <param name="type">Type.</param>
            /// <param name="list">List.</param>
        public override void GetInheritanceChain(Type type, List<Type> list) {
            #if NETFX_CORE

            Predicate<Type> namespaceExcluder = (Type t) => { 
				try {
					if (t.Namespace != null) {
						return !UnityTypeInspector.namespaceExclusions.Any(s => t.Namespace.Contains(s));
					} else {
                        return false;
					}
				} catch (Exception e) {
                    DebugLogger.LogError(e.ToString());
					return false;
				}
			};

            List<Assembly> assemblyList = GetAssemblyList().Result;
            Assembly[] assemblies = assemblyList.ToArray();
            Type[] derivedTypes = (from domainAssembly in assemblies
                                   from assemblyType in domainAssembly.GetTypes()
                                       where type.IsAssignableFrom(assemblyType)
                                       select assemblyType).ToArray();

            derivedTypes = Array.FindAll(derivedTypes, namespaceExcluder);

            foreach (Type derivedType in derivedTypes) {
				if (!list.Contains(derivedType)) {
                    DebugLogger.Log(derivedType.Name);

                    list.Add(derivedType);
				}
			}

            #else

            throw new NotImplementedException("This feature is not supported in non-UWP applications.");

            #endif
        }

        /// <summary>
        /// Gets the parent types.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="list">List.</param>
        public override void GetParentTypes(Type type, List<Type> list) {
            #if NETFX_CORE

            if (type == null) return;
			
			if (!list.Contains(type)) list.Add(type);
			
			// Add all implemented or inherited interfaces
			foreach (var i in type.GetInterfaces()) {
				if (!list.Contains(i)) list.Add(i);
			}
			
			// Add all inherited types
			var currentBaseType = type.GetTypeInfo().BaseType;
			
			while (currentBaseType != null) {
				if (!list.Contains(currentBaseType)) list.Add(currentBaseType);
				currentBaseType = currentBaseType.GetTypeInfo().BaseType;
			}

            #else

            throw new NotImplementedException("This feature is not supported in non-UWP applications.");

            #endif
        }

        /// <summary>
        /// Gets the method.
        /// </summary>
        /// <returns>The method.</returns>
        /// <param name="targetType">Target type.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="arguments">Arguments.</param>
        public override MethodInfo GetMethod(Type targetType, string methodName, object[] arguments) {
            #if NETFX_CORE

            MethodInfo[] methods = targetType.GetTypeInfo().GetAllMethods().ToArray<MethodInfo>();
            MethodInfo method = null;

            foreach (MethodInfo methodCandidate in methods) {
                if (methodCandidate.Name == methodName && UnityTypeInspector.MethodSignatureMatches(methodCandidate, arguments)) {
                    method = methodCandidate;
                    break;
                }
            }

            return method;

            #else

            throw new NotImplementedException("This feature is not supported in non-UWP applications.");

            #endif
        }

        /*//
        /// <summary>
		/// Finds out whether a method signature matches the passed in arguments.
		/// </summary>
		/// <returns><c>true</c>, if signature matches was methoded, <c>false</c> otherwise.</returns>
		/// <param name="method">Method.</param>
		/// <param name="arguments">Arguments.</param>
		public static bool MethodSignatureMatches(MethodInfo method, object[] arguments) {
            ParameterInfo[] methodParameters = method.GetParameters();

            if (methodParameters.Length != arguments.Length) return false;

            int i = 0;
            Type t, k = null;

            for (i = 0; i < methodParameters.Length; i++) {
                t = methodParameters[i].ParameterType;
                k = arguments[i].GetType();

                if (!t.Equals(k)) return false;
            }

            return true;
        }*/

        #endregion

        #if NETFX_CORE

        /// <summary>
        /// Provides a list of assemblies used in the current application. Based on code found originally at 
        /// https://www.reddit.com/r/markuxdev/comments/3z9xns/workaround_when_building_an_universal_windows/
        /// </summary>
        /// <returns>A list of assemblies used in the current application</returns>
        public static async Task<List<Assembly>> GetAssemblyList() {
            List<Assembly> assemblies = new List<Assembly>();

            var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();
            if (files == null) return assemblies;

            foreach (var file in files.Where(file => file.FileType == ".dll" || file.FileType == ".exe")) {
                try {
                    assemblies.Add(Assembly.Load(new AssemblyName(file.DisplayName)));
                } catch (Exception ex) {
                    DebugLogger.Log("Assembly file not found, failed to load or not authorized: " + file.DisplayName);
                    DebugLogger.Log(ex.Message);
                }

            }

            return assemblies;
        }

        #endif
    }
}
