using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace AlephVault.Unity.Layout
{
    namespace Utils
    {
        /// <summary>
        ///   This class acts as a namespace to holds several methods for components. Please refer to its methods.
        /// </summary>
        public static class Behaviours
        {
            /// <summary>
            ///   Exception to be raised when an object requires a parent but it has none.
            /// </summary>
            public class MissingParentException : AlephVault.Unity.Support.Types.Exception
            {
                public MissingParentException() { }
                public MissingParentException(string message) : base(message) { }
                public MissingParentException(string message, Exception inner) : base(message, inner) { }
            }

            /// <summary>
            ///   Exception to be raised when an object requires a certain component in its parent
            ///     but its parent has no such component.
            /// </summary>
            public class MissingComponentInParentException : AlephVault.Unity.Support.Types.Exception
            {
                public MissingComponentInParentException() { }
                public MissingComponentInParentException(string message) : base(message) { }
                public MissingComponentInParentException(string message, Exception inner) : base(message, inner) { }
            }

            /// <summary>
            ///   Exception to be raised when an object requires a certain component(s) on its
            ///     children, but the children do not have the required component(s).
            /// </summary>
            public class MissingComponentInChildrenException : AlephVault.Unity.Support.Types.Exception
            {
                public MissingComponentInChildrenException() { }
                public MissingComponentInChildrenException(string message) : base(message) { }
                public MissingComponentInChildrenException(string message, Exception inner) : base(message, inner) { }
            }

            /// <summary>
            ///   Exception to be raised when a field to be serialized is not found.
            /// </summary>
            public class UnserializableFieldException : AlephVault.Unity.Support.Types.Exception
            {
                public UnserializableFieldException() { }
                public UnserializableFieldException(string message) : base(message) { }
                public UnserializableFieldException(string message, System.Exception inner) : base(message, inner) { }
            }

            /// <summary>
            ///   Exception to be raised when there is a circular dependency among components
            ///     and so they cannot be sorted by dependencies (usually: to know how to invoke
            ///     certain method(s) from them in an appropriately sorted order).
            /// </summary>
            public class CircularDependencyUnsupportedException : AlephVault.Unity.Support.Types.Exception
            {
                public CircularDependencyUnsupportedException() { }
                public CircularDependencyUnsupportedException(string message) : base(message) { }
                public CircularDependencyUnsupportedException(string message, System.Exception inner) : base(message, inner) { }
            }

            /// <summary>
            ///   Requires a certain component in the parent of this behaviour's game object.
            /// </summary>
            /// <typeparam name="T">The type of component to require. It must be a subclass of <see cref="Component"/>.</typeparam>
            /// <param name="script">The behaviour. You will usually pass <c>this</c> here.</param>
            /// <returns>The found component, cast as type <typeparamref name="T"/>.</returns>
            /// <exception cref="MissingParentException" />
            /// <exception cref="MissingComponentInParentException" />
            public static T RequireComponentInParent<T>(MonoBehaviour script) where T : Component
            {
                return RequireComponentInParent<T>(script.gameObject);
            }

            /// <summary>
            ///   Requires a certain component in the parent of this game object.
            /// </summary>
            /// <typeparam name="T">The type of component to require. It must be a subclass of <see cref="Component"/>.</typeparam>
            /// <param name="current">The object. You will usually pass <c>this.gameObject</c> here.</param>
            /// <returns>The found component, cast as type <typeparamref name="T"/>.</returns>
            /// <exception cref="MissingParentException" />
            /// <exception cref="MissingComponentInParentException" />
            public static T RequireComponentInParent<T>(GameObject current) where T : Component
            {
                try
                {
                    Transform parentTransform = current.transform.parent;
                    GameObject parentGameObject = (parentTransform != null) ? parentTransform.gameObject : null;
                    T component = parentGameObject.GetComponent<T>();
                    if (component == null)
                    {
                        throw new MissingComponentInParentException("Current object's parent needs a component of type " + typeof(T).FullName);
                    }
                    else
                    {
                        return component;
                    }
                }
                catch (NullReferenceException)
                {
                    throw new MissingParentException("Current object needs a parent object");
                }
            }

            /// <summary>
            ///   Requires a certain component among the children (or descendants) of this behaviour's game object.
            /// </summary>
            /// <typeparam name="T">The type of component to require. It must be a subclass of <see cref="Component"/>.</typeparam>
            /// <param name="current">The behaviour. You will usually pass <c>this</c> here.</param>
            /// <param name="includeInactive">If set, also includes the inactive objects in the search</param>
            /// <returns>The found component, cast as type <typeparamref name="T"/>.</returns>
            /// <exception cref="MissingComponentInChildrenException" />
            public static T RequireComponentInChildren<T>(MonoBehaviour current, bool includeInactive = true) where T : Component
            {
                return RequireComponentInChildren<T>(current.gameObject, includeInactive);
            }

            /// <summary>
            ///   Requires a certain component among the children (or descendants) of this game object.
            /// </summary>
            /// <typeparam name="T">The type of component to require. It must be a subclass of <see cref="Component"/>.</typeparam>
            /// <param name="current">The object. You will usually pass <c>this.gameObject</c> here.</param>
            /// <param name="includeInactive">If set, also includes the inactive objects in the search</param>
            /// <returns>The found component, cast as type <typeparamref name="T"/>.</returns>
            /// <exception cref="MissingComponentInChildrenException" />
            public static T RequireComponentInChildren<T>(GameObject current, bool includeInactive = true) where T : Component
            {
                T[] components = current.GetComponentsInChildren<T>(includeInactive);
                if (components.Length == 0)
                {
                    throw new MissingComponentInChildrenException("Current object's children must, at least, have one component of type " + typeof(T).FullName);
                }
                else
                {
                    return components.First();
                }
            }

            /// <summary>
            ///   Requires at least N copies of a certain component among the children (or descendants) of this behaviour's game object.
            /// </summary>
            /// <typeparam name="T">The type of component to require. It must be a subclass of <see cref="Component"/>.</typeparam>
            /// <param name="script">The behaviour. You will usually pass <c>this</c> here.</param>
            /// <param name="howMany">The minimum amount of components to retrieve. It is an error if there are less than that.</param>
            /// <param name="includeInactive">This argument has the same meaning as in <see cref="Component.GetComponentsInChildren(Type, bool)"/>.</param>
            /// <returns>The found components, cast as type <typeparamref name="T"/>.</returns>
            /// <exception cref="MissingComponentInChildrenException" />
            public static T[] RequireComponentsInChildren<T>(MonoBehaviour script, uint howMany, bool includeInactive = true) where T : Component
            {
                return RequireComponentsInChildren<T>(script.gameObject, howMany, includeInactive);
            }

            /// <summary>
            ///   Requires at least N copies of a certain component among the children (or descendants) of this game object.
            /// </summary>
            /// <typeparam name="T">The type of component to require. It must be a subclass of <see cref="Component"/>.</typeparam>
            /// <param name="current">The object. You will usually pass <c>this.gameObject</c> here.</param>
            /// <param name="howMany">The minimum amount of components to retrieve. It is an error if there are less than that.</param>
            /// <param name="includeInactive">This argument has the same meaning as in <see cref="Component.GetComponentsInChildren(Type, bool)"/>.</param>
            /// <returns>The found components, cast as type <typeparamref name="T"/>.</returns>
            /// <exception cref="MissingComponentInChildrenException" />
            public static T[] RequireComponentsInChildren<T>(GameObject current, uint howMany, bool includeInactive = true) where T : Component
            {
                T[] components = current.GetComponentsInChildren<T>(includeInactive);
                if (components == null || components.Length < howMany)
                {
                    throw new MissingComponentInChildrenException("Current object's children must, at least, have " + howMany + " component(s) of type " + typeof(T).FullName);
                }
                else
                {
                    T[] result = new T[howMany];
                    Array.ConstrainedCopy(components, 0, result, 0, (int) howMany);
                    return result;
                }
            }

            /// <summary>
            ///   Adds a component to the game object. It also initializes its properties (specially for those marked with <see cref="SerializeField"/>).
            /// </summary>
            /// <remarks>
            ///   <para>When this method completes its execution, <c>Awake()</c> and <c>Start()</c> will be invoked accordingly.</para>
            ///   <para>Please note! This feature uses reflection! It will be slow. Use it with caution!</para>
            /// </remarks>
            /// <typeparam name="T">The type of component to add.</typeparam>
            /// <param name="gameObject">The object to which add the component.</param>
            /// <param name="data">An arbitrary map of string => object properties to initialize.</param>
            /// <returns>The newly added component.</returns>
            /// <exception cref="UnserializableFieldException" />
            public static T AddComponent<T>(GameObject gameObject, Dictionary<string, object> data = null) where T : Component
            {
                if (data == null)
                {
                    return gameObject.AddComponent<T>();
                }
                else
                {
                    return EnsureInactive<T>(gameObject, delegate () {
                        T component = gameObject.AddComponent<T>();
                        SetObjectFieldValues(component, data);
                        return component;
                    });
                }
            }

            /// <summary>
            ///   Turns an object as inactive, runs certain custom code, and sets the object back to its previous active/inactive state.
            ///   Returns whatever value is returned by the custom code.
            /// </summary>
            /// <param name="gameObject">The object to blink out while running the custom code.</param>
            /// <param name="action">The custom code to run.</param>
            public static T EnsureInactive<T>(GameObject gameObject, Func<T> action)
            {
                bool active = gameObject.activeSelf;
                gameObject.SetActive(false);
                T result = action();
                gameObject.SetActive(active);
                return result;
            }

            /// <summary>
            ///   Sets, in bulk, a lot of properties on certain component or asset.
            /// </summary>
            /// <remarks>
            ///   <para>Please note! This feature uses reflection! It will be slow. Use it with caution!</para>
            /// </remarks>
            /// <param name="target">The component on which the properties will be set.</param>
            /// <param name="data">The source of the properties to set.</param>
            /// <exception cref="UnserializableFieldException" />
            public static void SetObjectFieldValues(UnityEngine.Object target, Dictionary<string, object> data)
            {
                Type targetType = target.GetType();
                BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                foreach (KeyValuePair<string, object> pair in data)
                {
                    FieldInfo field = null;
                    Type currentType = targetType;
                    while (field == null && currentType != null)
                    {
                        field = currentType.GetField(pair.Key, all);
                        if (field != null && (field.IsPublic || field.IsDefined(typeof(SerializeField), true)))
                        {
                            break;
                        }
                        else
                        {
                            field = null;
                            currentType = currentType.BaseType;
                        }
                    }
                    if (field != null)
                    {
                        field.SetValue(target, pair.Value);
                    }
                    else
                    {
                        throw new UnserializableFieldException("The field " + pair.Key + " cannot be populated for type " + targetType.FullName);
                    }
                }
            }

            /// <summary>
            ///   Gets all the dependencies of the object's type, considering its <see cref="RequireComponent"/> tags.
            /// </summary>
            /// <remarks>
            ///   This considers component 1, 2, and 3 in each <see cref="RequireComponent"/> tag, and excludes <c>null</c>.
            /// </remarks>
            /// <param name="component">The component to query its dependencies.</param>
            /// <returns>A set of dependencies.</returns>
            public static HashSet<Type> GetDependencies(Component component)
            {
                return GetDependencies(component.GetType());
            }

            /// <summary>
            ///   Gets all the dependencies of the given type param, considering its <see cref="RequireComponent"/> tags.
            /// </summary>
            /// <remarks>
            ///   This considers component 1, 2, and 3 in each <see cref="RequireComponent"/> tag, and excludes <c>null</c>.
            /// </remarks>
            /// <typeparam name="C">The component type to query its dependencies.</typeparam>
            /// <returns>A set of dependencies.</returns>
            public static HashSet<Type> GetDependencies<C>() where C : Component
            {
                return GetDependencies(typeof(C));
            }

            /// <summary>
            ///   Gets all the dependencies of the given type argument, considering its <see cref="RequireComponent"/> tags.
            /// </summary>
            /// <remarks>
            ///   This considers component 1, 2, and 3 in each <see cref="RequireComponent"/> tag, and excludes <c>null</c>.
            /// </remarks>
            /// <param name="componentType">The component type to query its dependencies.</param>
            /// <returns>A set of dependencies.</returns>
            private static HashSet<Type> GetDependencies(Type componentType)
            {
                IEnumerable<RequireComponent> attributes = (from attribute in componentType.GetCustomAttributes(typeof(RequireComponent), false) select (attribute as RequireComponent));
                HashSet<Type> types = new HashSet<Type>();
                foreach (var attribute in attributes)
                {
                    types.Add(attribute.m_Type0);
                    types.Add(attribute.m_Type1);
                    types.Add(attribute.m_Type2);
                }
                types.Remove(null);
                return types;
            }

            /// <summary>
            ///   Sorts a list of components according to how much do they depend on each other. The less-dependent can be found at start.
            /// </summary>
            /// <remarks>An exception will be raised if there are circular dependencies here.</remarks>
            /// <param name="components">The components to sort by their dependencies.</param>
            /// <exception cref="CircularDependencyUnsupportedException" />public static T RequireComponentInParent<T>(MonoBehaviour script)
            /// <returns>An array of components (appropriately sorted from less-dependent to more-dependent).</returns>
            public static Component[] SortByDependencies(Component[] components)
            {
                HashSet<Type> consideredComponentTypes = new HashSet<Type>(from component in components select component.GetType());
                List<Component> sourceComponentsList = new List<Component>(components);
                List<Component> endComponentsList = new List<Component>();
                HashSet<Type> fetchedTypes = new HashSet<Type>();

                while(true)
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
                    foreach(var component in sourceComponentsList)
                    {
                        HashSet<Type> dependencies = new HashSet<Type>(GetDependencies(component).Intersect(consideredComponentTypes));
                        if (fetchedTypes.IsSupersetOf(dependencies))
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
                        throw new CircularDependencyUnsupportedException("Circular dependencies among components is unsupported when trying to sort them by dependencies");
                    }
                }

                // This is the end result. Elements were added in-order.
                return endComponentsList.ToArray();
            }
        }
    }
}