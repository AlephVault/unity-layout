using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlephVault.Unity.Layout
{
    namespace Utils
    {
        /// <summary>
        ///   This class acts as a namespace to holds several methods for assets. Please refer to its methods.
        /// </summary>
        public static class Assets
        {
            /**
             * This class can track arbitrary dependencies from a particular array of objects.
             * This array of objects is intended to serve as a set of data bundles for different
             *   ScriptableObject or regular (non-GameObject) classes. This class will work in
             *   consideration with different parameters involving:
             *   
             * 1. The T class of the T[] input array.
             * 2. The A class, descendant of Depends (descending from System.Attribute), to seek.
             * 3. An exception class, being by default the exception defined in this class, and
             *      mandatory being subclass of such exception type.
             */
            
            /// <summary>
            ///   Exception to be raised on dependency errors.
            /// </summary>
            public class DependencyException : Support.Types.Exception
            {
                public DependencyException(string message) : base(message) { }
            }

            /// <summary>
            ///   Exception to be raised when the specified main component is not present among
            ///     the given list of components.
            /// </summary>
            public class MainComponentException : DependencyException
            {
                public MainComponentException(string message) : base(message) { }
            }

            /// <summary>
            ///   This is an attribute to be used like <see cref="UnityEngine.RequireComponent"/> by any
            ///     kind of objects. Most likely, this will only be used by Unity's ScriptableObject classes.
            /// </summary>
            public abstract class Depends : Attribute
            {
                private Type dependency;

                public Depends(Type dependency)
                {
                    Type baseDependency = BaseDependency();
                    if (!baseDependency.IsAssignableFrom(dependency))
                    {
                        throw new DependencyException(string.Format("Invalid requirement class. It must descend from {0} for {1} attribute", baseDependency.FullName, GetType().FullName));
                    }
                    this.dependency = dependency;
                }

                protected abstract Type BaseDependency();

                public Type Dependency
                {
                    get { return dependency; }
                }
            }

            private static void throwException(Type type, string message)
            {
                throw (Exception)Activator.CreateInstance(type, new object[] { message });
            }

            private static HashSet<Type> GetDependencies(Type attributeType, Type type)
            {
                return new HashSet<Type>(
                    from attribute in type.GetCustomAttributes(attributeType, true) select ((Depends)attribute).Dependency
                );
            }

            /// <summary>
            ///   Gets the dependencies of a given type, according to a given attribute type, and raising a <c>DependencyException</c> exception on dependency error.
            /// </summary>
            /// <typeparam name="A">The attribute type. A subclass of <see cref="Depends"/>.</typeparam>
            /// <typeparam name="T">The type being queried.</typeparam>
            /// <returns>A set of types that are dependencies of the queried type.</returns>
            public static HashSet<Type> GetDependencies<A, T>() where A : Depends
            {
                return GetDependencies(typeof(A), typeof(T));
            }

            private static void CheckAssignability(Type attributeType, Type type, Type exceptionType)
            {
                if (!typeof(Depends).IsAssignableFrom(attributeType))
                {
                    throwException(exceptionType, "Invalid attribute class / component class combination - attribute class must descend from AssetsLayout.Depends");
                }
            }

            /// <summary>
            ///   Gets the dependencies of a given type, according to a given attribute type, and raising a particular exception on dependency error.
            /// </summary>
            /// <typeparam name="A">The attribute type. A subclass of <see cref="Depends"/>.</typeparam>
            /// <typeparam name="E">The exception type. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="type">The type being queried.</param>
            /// <returns>A set of types that are dependencies of the queried type.</returns>
            public static HashSet<Type> GetDependencies<A, E>(Type type) where A : Attribute where E : DependencyException
            {
                Type attributeType = typeof(A);
                Type exceptionType = typeof(E);
                CheckAssignability(attributeType, type, exceptionType);
                return GetDependencies(attributeType, type);
            }

            /// <summary>
            ///   Gets the dependencies of a given type, according to a given attribute type, and raising a <c>DependencyException</c> exception on dependency error.
            /// </summary>
            /// <typeparam name="A">The attribute type. A subclass of <see cref="Depends"/>.</typeparam>
            /// <param name="type">The type being queried.</param>
            /// <returns>A set of types that are dependencies of the queried type.</returns>
            public static HashSet<Type> GetDependencies<A>(Type type) where A : Attribute
            {
                Type attributeType = typeof(A);
                Type exceptionType = typeof(DependencyException);
                CheckAssignability(attributeType, type, exceptionType);
                return GetDependencies(attributeType, type);
            }

            /// <summary>
            ///   Gets the dependencies of a given component's type, according to a given attribute type, and raising a particular exception on dependency error.
            /// </summary>
            /// <typeparam name="A">The attribute type. A subclass of <see cref="Depends"/>.</typeparam>
            /// <typeparam name="E">The exception type. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="component">The component whose type is being queried.</param>
            /// <returns>A set of types that are dependencies of the queried type.</returns>
            public static HashSet<Type> GetDependencies<A, E>(object component) where A : Attribute where E : DependencyException
            {
                return GetDependencies<A, E>(component.GetType());
            }

            /// <summary>
            ///   Gets the dependencies of a given component's type, according to a given attribute type, and raising a <c>DependencyException</c> exception on dependency error.
            /// </summary>
            /// <typeparam name="A">The attribute type. A subclass of <see cref="Depends"/>.</typeparam>
            /// <param name="component">The component whose type is being queried.</param>
            /// <returns>A set of types that are dependencies of the queried type.</returns>
            public static HashSet<Type> GetDependencies<A>(object component) where A : Attribute
            {
                return GetDependencies<A, DependencyException>(component.GetType());
            }

            /// <summary>
            ///   Flattens/sorts the components by dependencies. It may throw an error if some dependencies are unmet among these components.
            /// </summary>
            /// <typeparam name="T">The (common ancestor) type of components to pass.</typeparam>
            /// <typeparam name="A">The attribute to consider when fetching dependencies. A subclass of <see cref="Depends"/>.</typeparam>
            /// <typeparam name="E">The exception to raise on error. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="components">The components to sort.</param>
            /// <param name="errorOnMissingDependency">If <c>true</c>, all the components' dependencies have to be present among them, or an error will be raised.</param>
            /// <returns>The sorted components list.</returns>
            public static T[] FlattenDependencies<T, A, E>(T[] components, bool errorOnMissingDependency = true) where A : Depends where E : DependencyException
            {
                HashSet<Type> consideredComponentTypes = new HashSet<Type>(from component in components select component.GetType());
                List<T> sourceComponentsList = new List<T>(components);
                List<T> endComponentsList = new List<T>();
                HashSet<Type> fetchedTypes = new HashSet<Type>();

                while (true)
                {
                    // We end the loop if there is nothing else in the
                    //   source. The fetching terminated.
                    if (sourceComponentsList.Count == 0) break;

                    // On each iteration we will try adding at least one
                    //   element to the final components list. Otherwise, we
                    //   fail because the underlying reason is a circular
                    //   dependency.
                    bool foundTypeToAdd = false;

                    // We iterate over all the components trying to find
                    //   one to add. The criterion to accept one to add
                    //   is that either it doesn't have dependencies or
                    //   all its dependencies have already been processed.
                    foreach (var component in sourceComponentsList)
                    {
                        // Getting dependencies of the component.
                        HashSet<Type> componentDependencies = GetDependencies<A, E>(component);
                        if (errorOnMissingDependency && componentDependencies.Except(consideredComponentTypes).Count() > 0)
                        {
                            throwException(typeof(E), "At least one component has a dependency requirement not satisfied in the given input list");
                        }

                        if (fetchedTypes.IsSupersetOf(componentDependencies))
                        {
                            // We mark that we found. Then we remove from source, add to end, and mark the type as fetched.
                            foundTypeToAdd = true;
                            sourceComponentsList.Remove(component);
                            endComponentsList.Add(component);
                            fetchedTypes.Add(component.GetType());
                            break;
                        }
                    }

                    // If no element was found to be added, we raise an
                    //   exception because we found a circular dependency.
                    if (!foundTypeToAdd)
                    {
                        throwException(typeof(E), "Circular dependencies among components is unsupported when trying to sort them by dependencies");
                    }
                }

                // This is the end result. Elements were added in-order.
                return endComponentsList.ToArray();
            }

            /// <summary>
            ///   Flattens/sorts the components by dependencies. It may throw a DependencyException error if some dependencies are unmet among these components.
            /// </summary>
            /// <typeparam name="T">The (common ancestor) type of components to pass.</typeparam>
            /// <typeparam name="A">The attribute to consider when fetching dependencies. A subclass of <see cref="Depends"/>.</typeparam>
            /// <param name="components">The components to sort.</param>
            /// <param name="errorOnMissingDependency">If <c>true</c>, all the components' dependencies have to be present among them, or an error will be raised.</param>
            /// <returns>The sorted components list.</returns>
            public static T[] FlattenDependencies<T, A>(T[] componentsList, bool errorOnMissingDependency = true) where A : Depends
            {
                return FlattenDependencies<T, A, DependencyException>(componentsList, errorOnMissingDependency);
            }

            /// <summary>
            ///   Avoids duplicate dependencies among components. This ensures that there are no two or more components
            ///     of the same type in a components list (quite like the Inspector ensures that regarding game objects
            ///     and their components). A particular exception will be raised on duplicates.
            /// </summary>
            /// <typeparam name="T">The (common ancestor) type of components to pass.</typeparam>
            /// <typeparam name="E">The exception to raise on error. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="componentsList">The components to check.</param>
            /// <returns>The dictionary of components by their types.</returns>
            public static Dictionary<Type, T> AvoidDuplicateDependencies<T, E>(T[] componentsList) where E : DependencyException
            {
                Dictionary<Type, T> dictionary = new Dictionary<Type, T>();
                foreach(T component in componentsList)
                {
                    Type type = component.GetType();
                    if (dictionary.ContainsKey(type))
                    {
                        throwException(typeof(E), "Cannot add more than one component instance per component type");
                    }
                    dictionary[type] = component;
                }
                return dictionary;
            }

            /// <summary>
            ///   Avoids duplicate dependencies among components. This ensures that there are no two or more components
            ///     of the same type in a components list (quite like the Inspector ensures that regarding game objects
            ///     and their components. A <see cref="DependencyException"/> exception will be raised on duplicates.
            /// </summary>
            /// <typeparam name="T">The (common ancestor) type of components to pass.</typeparam>
            /// <param name="componentsList">The components to check.</param>
            /// <returns>The dictionary of components by their types.</returns>
            public static Dictionary<Type, T> AvoidDuplicateDependencies<T>(T[] componentsList)
            {
                return AvoidDuplicateDependencies<T, DependencyException>(componentsList);
            }

            /// <summary>
            ///   Checks that all the components in a list have their dependencies satisfied by the components in the second list. If there are unsatisfied dependencies,
            ///     a particular exception will be raised.
            /// </summary>
            /// <typeparam name="TargetType">The type with the dependent components.</typeparam>
            /// <typeparam name="DependencyType">The type with the dependency components.</typeparam>
            /// <typeparam name="A">The attribute type to consider. A subclass of <see cref="Depends"/>.</typeparam>
            /// <typeparam name="E">The exception to raise. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="componentsList">The main components that have dependencies.</param>
            /// <param name="dependencies">The components that satisfy those dependencies.</param>
            public static void CrossCheckDependencies<TargetType, DependencyType, A, E>(TargetType[] componentsList, DependencyType[] dependencies) where A : Depends where E : DependencyException
            {
                HashSet<Type> requiredDependencies = new HashSet<Type>();
                foreach (TargetType component in componentsList)
                {
                    foreach (A attribute in component.GetType().GetCustomAttributes(typeof(A), true))
                    {
                        requiredDependencies.Add(attribute.Dependency);
                    }
                }
                HashSet<Type> installed = new HashSet<Type>(from dependency in dependencies select dependency.GetType());
                HashSet<Type> unsatisfiedDependencies = new HashSet<Type>(requiredDependencies.Except(installed));
                if (unsatisfiedDependencies.Count > 0)
                {
                    if (unsatisfiedDependencies.Count == 1)
                    {
                        throwException(typeof(E), "Unsatisfied dependency: " + unsatisfiedDependencies.First().FullName);
                    }
                    else
                    {
                        throwException(typeof(E), "Unsatisfied dependencies: " + string.Join(",", (from unsatisfiedDependency in unsatisfiedDependencies select unsatisfiedDependency.FullName).ToArray()));
                    }
                }
            }

            /// <summary>
            ///   Checks that all the components in a list have their dependencies satisfied by the components in the second list. If there are unsatisfied dependencies,
            ///     a <see cref="DependencyException" /> exception will be raised.
            /// </summary>
            /// <typeparam name="TargetType">The type with the dependent components.</typeparam>
            /// <typeparam name="DependencyType">The type with the dependency components.</typeparam>
            /// <typeparam name="A">The attribute type to consider. A subclass of <see cref="Depends"/>.</typeparam>
            /// <param name="componentsList">The main components that have dependencies.</param>
            /// <param name="dependencies">The components that satisfy those dependencies.</param>
            public static void CrossCheckDependencies<TargetType, DependencyType, A>(TargetType[] componentsList, DependencyType[] dependencies) where A : Depends
            {
                CrossCheckDependencies<TargetType, DependencyType, A, DependencyException>(componentsList, dependencies);
            }

            /// <summary>
            ///   Checks that all the components in a list have their dependencies satisfied by the component in the second argument. If there are unsatisfied dependencies,
            ///     a particular exception will be raised.
            /// </summary>
            /// <typeparam name="TargetType">The type with the dependent components.</typeparam>
            /// <typeparam name="DependencyType">The type with the dependency components.</typeparam>
            /// <typeparam name="A">The attribute type to consider. A subclass of <see cref="Depends"/>.</typeparam>
            /// <typeparam name="E">The exception to raise. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="componentsList">The main components that have dependencies.</param>
            /// <param name="dependency">The component that satisfy those dependencies.</param>
            public static void CrossCheckDependencies<TargetType, DependencyType, A, E>(TargetType[] componentsList, DependencyType dependency) where A : Depends where E : DependencyException
            {
                CrossCheckDependencies<TargetType, DependencyType, A, E>(componentsList, new DependencyType[] { dependency });
            }

            /// <summary>
            ///   Checks that all the components in a list have their dependencies satisfied by the component in the second argument. If there are unsatisfied dependencies,
            ///     a <see cref="DependencyException"/> exception will be raised.
            /// </summary>
            /// <typeparam name="TargetType">The type with the dependent components.</typeparam>
            /// <typeparam name="DependencyType">The type with the dependency components.</typeparam>
            /// <typeparam name="A">The attribute type to consider. A subclass of <see cref="Depends"/>.</typeparam>
            /// <param name="componentsList">The main components that have dependencies.</param>
            /// <param name="dependency">The component that satisfy those dependencies.</param>
            public static void CrossCheckDependencies<TargetType, DependencyType, A>(TargetType[] componentsList, DependencyType dependency) where A : Depends
            {
                CrossCheckDependencies<TargetType, DependencyType, A, DependencyException>(componentsList, dependency);
            }

            /// <summary>
            ///   Checks that the component in the first argument have its dependencies satisfied by the components in the second list. If there are unsatisfied dependencies,
            ///     a particular exception will be raised.
            /// </summary>
            /// <typeparam name="TargetType">The type with the dependent components.</typeparam>
            /// <typeparam name="DependencyType">The type with the dependency components.</typeparam>
            /// <typeparam name="A">The attribute type to consider. A subclass of <see cref="Depends"/>.</typeparam>
            /// <typeparam name="E">The exception to raise. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="component">The main components that have dependencies.</param>
            /// <param name="dependencies">The component that satisfy those dependencies.</param>
            public static void CrossCheckDependencies<TargetType, DependencyType, A, E>(TargetType component, DependencyType[] dependencies) where A : Depends where E : DependencyException
            {
                CrossCheckDependencies<TargetType, DependencyType, A, E>(new TargetType[] { component }, dependencies);
            }

            /// <summary>
            ///   Checks that the component in the first argument have its dependencies satisfied by the components in the second list. If there are unsatisfied dependencies,
            ///     a <see cref="DependencyException"/> exception will be raised.
            /// </summary>
            /// <typeparam name="TargetType">The type with the dependent components.</typeparam>
            /// <typeparam name="DependencyType">The type with the dependency components.</typeparam>
            /// <typeparam name="A">The attribute type to consider. A subclass of <see cref="Depends"/>.</typeparam>
            /// <param name="component">The main components that have dependencies.</param>
            /// <param name="dependencies">The component that satisfy those dependencies.</param>
            public static void CrossCheckDependencies<TargetType, DependencyType, A>(TargetType component, DependencyType[] dependencies) where A : Depends
            {
                CrossCheckDependencies<TargetType, DependencyType, A, DependencyException>(component, dependencies);
            }

            /// <summary>
            ///   Checks that the component in the first argument have its dependencies satisfied by the component in the second argument. If there are unsatisfied dependencies,
            ///     a particular exception will be raised.
            /// </summary>
            /// <typeparam name="TargetType">The type with the dependent components.</typeparam>
            /// <typeparam name="DependencyType">The type with the dependency components.</typeparam>
            /// <typeparam name="A">The attribute type to consider. A subclass of <see cref="Depends"/>.</typeparam>
            /// <typeparam name="E">The exception to raise. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="component">The main components that have dependencies.</param>
            /// <param name="dependency">The component that satisfy those dependencies.</param>
            public static void CrossCheckDependencies<TargetType, DependencyType, A, E>(TargetType component, DependencyType dependency) where A : Depends where E : DependencyException
            {
                CrossCheckDependencies<TargetType, DependencyType, A, E>(new TargetType[] { component }, new DependencyType[] { dependency });
            }

            /// <summary>
            ///   Checks that the component in the first argument have its dependencies satisfied by the component in the second argument. If there are unsatisfied dependencies,
            ///     a <see cref="DependencyException"/> exception will be raised.
            /// </summary>
            /// <typeparam name="TargetType">The type with the dependent components.</typeparam>
            /// <typeparam name="DependencyType">The type with the dependency components.</typeparam>
            /// <typeparam name="A">The attribute type to consider. A subclass of <see cref="Depends"/>.</typeparam>
            /// <param name="component">The main components that have dependencies.</param>
            /// <param name="dependency">The component that satisfy those dependencies.</param>
            public static void CrossCheckDependencies<TargetType, DependencyType, A>(TargetType component, DependencyType dependency) where A : Depends
            {
                CrossCheckDependencies<TargetType, DependencyType, A, DependencyException>(component, dependency);
            }

            /// <summary>
            ///   Checks that a certain component instance is contained among a specified components list. A particular exception will be
            ///     raised if the component is not in the list.
            /// </summary>
            /// <typeparam name="T">The components (common ancestor) type.</typeparam>
            /// <typeparam name="E">The exception type. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="components">The list of components to search in.</param>
            /// <param name="mainComponent">The component to search.</param>
            public static void CheckMainComponent<T, E>(T[] components, T mainComponent) where E : MainComponentException
            {
                if (!components.Contains(mainComponent))
                {
                    throwException(typeof(E), string.Format("An instance of {0} must be selected as main component", typeof(T).FullName));
                }
            }

            /// <summary>
            ///   Checks that a certain component instance is contained among a specified components list. A <see cref="DependencyException"/>
            ///     exception will be raised if the component is not in the list.
            /// </summary>
            /// <typeparam name="T">The components (common ancestor) type.</typeparam>
            /// <param name="components">The list of components to search in.</param>
            /// <param name="mainComponent">The component to search.</param>
            public static void CheckMainComponent<T>(T[] components, T mainComponent)
            {
                CheckMainComponent<T, MainComponentException>(components, mainComponent);
            }

            /// <summary>
            ///   Checks that a certain value is not null. This involves checking the requirement of a field
            ///     being present. If not, a particular exception will be raised.
            /// </summary>
            /// <typeparam name="T">The component type to check against null.</typeparam>
            /// <typeparam name="E">The exception type. A subclass of <see cref="DependencyException"/>.</typeparam>
            /// <param name="component">The component to check against null.</param>
            /// <param name="fieldName">The field name. Just for informative purposes.</param>
            public static void CheckPresence<T, E>(T component, string fieldName = "") where E : DependencyException
            {
                if (component == null)
                {
                    fieldName = fieldName != "" ? fieldName : string.Format("[unspecified field of type {0}] is required: it must not be null", component.GetType().Name);
                    throwException(typeof(E), fieldName);
                }
            }

            /// <summary>
            ///   Checks that a certain value is not null. This involves checking the requirement of a field
            ///     being present. If not, a <see cref="DependencyException"/> exception will be raised.
            /// </summary>
            /// <typeparam name="T">The component type to check against null.</typeparam>
            /// <param name="component">The component to check against null.</param>
            /// <param name="fieldName">The field name. Just for informative purposes.</param>
            public static void CheckPresence<T>(T component, string fieldName = "")
            {
                CheckPresence<T, DependencyException>(component);
            }
        }
    }
}
