# Unity Layout

This package contains layout helper classes to quickly manage components, both in GameObjects and inside some ScriptableObject settings.

# Install

This package is not available in any UPM server. You must install it in your project like this:

1. In Unity, with your project open, open the Package Manager.
2. Either refer this Github project: https://github.com/AlephVault/unity-layout.git or clone it locally and refer it from disk.
3. Also, the following packages are dependencies you need to install accordingly (in the same way and also ensuring all the recursive dependencies are satisfied):

     - https://github.com/AlephVault/unity-support.git

# Usage

## Utils

There are only two static classes in this package. Both of them present in the `AlephVault.Unity.Layout.Utils`.

### `Assets`

This is a static class to deal with assets and the dependency between them (even for non-Behaviour objects). The methods are:

- `public static HashSet<Type> Assets.GetDependencies<A, T>()`: Returns all the dependencies of an object following the attributes of type `A` defined in a type `T`. This means that invoking `Assets.GetDependencies<MyAttribute, SomeType>()` will collect all the types defined in the `SomeType` type assigned to all the `MyAttribute` attribute instances defined in that type. `MyAttribute` inherits of `Assets.Depends`, which is a class that defines a `Dependency` attribute, which is queried in this method.
- `public static HashSet<Type> Assets.GetDependencies<A>(Type t)`: An alternate version of the same method. The type is instead passed as a regular parameter.
- `public static HashSet<Type> Assets.GetDependencies<T>(object component)`:  An alternate version of the same method. It takes the component's type to invoke the previous version.
- `public static T[] FlattenDependencies<T, A>(T[] componentsList, bool errorOnMissingDependency = true)`: Given the component list and the attribute type, it flattens and sorts them from less-interdependent (the first element will have zero dependencies) to most dependent (the last element will be the most dependent one from that list). If `errorOnMissingDependency` is true, then `DependencyException` will be raised if the objects in the list depend on one or more dependency types not listed there. Circular dependencies will be raised always.
- `public static T[] FlattenDependencies<T, A, E>(T[] componentsList, bool errorOnMissingDependency = true)`: Same idea but throws `E` exception in those cases.
- `public static Dictionary<Type, T> AvoidDuplicateDependencies<T>(T[] componentsList)`: Throws a `DependencyException` if there are duplicate dependencies (i.e. dependency attributes defining the same type twice or more).
- `public static Dictionary<Type, T> AvoidDuplicateDependencies<T, E>(T[] componentsList)`: Throws a `E` exception (descendant of `DependencyException`) if there are duplicate dependencies (i.e. dependency attributes defining the same type twice or more).
- `public static void CrossCheckDependencies<TargetType, DependencyType, A, E>(TargetType[] componentsList, DependencyType[] dependencies)`: For an exception `E` derived from `DependencyException` and an attribute type `A` is derived from `Depends`, it checks that all the dependencies in `componentsList` are satisfied by one or more dependencies in `dependencies`.
- `public static void CrossCheckDependencies<TargetType, DependencyType, A>(TargetType[] componentsList, DependencyType[] dependencies)`: Same idea but `E` is `DependencyException`.
- `public static void CrossCheckDependencies<TargetType, DependencyType, A, E>(TargetType[] componentsList, DependencyType dependency)`: Same idea but only one dependency object is passed. The exception `E` derives of `DependencyException`.
- `public static void CrossCheckDependencies<TargetType, DependencyType, A>(TargetType[] componentsList, DependencyType dependency)`: Same idea but only one dependency object is passed and the exception type is `DependencyException`.
- `public static void CrossCheckDependencies<TargetType, DependencyType, A, E>(TargetType component, DependencyType[] dependencies)`: Same idea but only one component is checked against the list of dependencies. The exception `E` derives of `DependencyException`.
- `public static void CrossCheckDependencies<TargetType, DependencyType, A>(TargetType component, DependencyType[] dependencies)`: Same idea but only one component is checked against the list of dependencies and also the `DependencyException` is used as exception type.
- `public static void CrossCheckDependencies<TargetType, DependencyType, A, E>(TargetType component, DependencyType dependency)`:  Same idea but only one component is checked against only one dependency. The exception `E` derives of `DependencyException`.
- `public static void CrossCheckDependencies<TargetType, DependencyType, A>(TargetType component, DependencyType dependency)`: Same idea but only one component is checked against only one dependency, and the exception type is `DependencyException`.
- `public static void CheckMainComponent<T, E>(T[] components, T mainComponent)`: Checks that the `mainComponent` is among the `components`. Raises an exception of type `E` otherwise. The exception `E` derives of `DependencyException`.
- `public static void CheckMainComponent<T>(T[] components, T mainComponent)`: Checks that the `mainComponent` is among the `components`. Raises an exception of type `DependencyException`.
- `public static void CheckPresence<C, E>(C component, string fieldName = "")`: Checks that the component is _not_ null. Raises a detailed error of type `E`,  derived of `MainComponentException`.
- `public static void CheckPresence<C>(C component, string fieldName = "")`: Checks that the component is _not_ null. Raises a detailed error of type `MainComponentException`.

_In all these methods, T, TargetType and DependencyType are derived from UnityEngine.Object._

#### `Assets.DependencyException`

An exception triggered when a dependency is not satisfied or there's an error processing the dependencies of an object.

#### `Assets.MainComponentException`

An exception triggered when the object chosen as _main component_ (when this applies) is not among the satisfied dependencies.

### `Behaviours`

This is another static class with utility methods. In these methods, `T` is a `Component`-derived type. The methods are:

- `public static T RequireComponentInParent<T>(MonoBehaviour script)`: Requires that the parent object (of the gameObject this Behaviour is attached to) has a component of type `T`. Raises `MissingComponentInParentException` on absence.
- `public static T RequireComponentInParent<T>(GameObject script)`: Requires that the parent object has a component of type `T`. Raises `MissingComponentInParentException` on absence.
- `public static T RequireComponentInChildren<T>(MonoBehaviour current)`: Requires that one of the children (as in `current.GetComponentInChildren<T>`) has a required component of type `T`. Raises `MissingComponentInChildrenException` on absence.
- `public static T RequireComponentInChildren<T>(GameObject current)`: Same idea but this one is called on the gameObject directly.
- `public static T[] RequireComponentsInChildren<T>(MonoBehaviour script, uint howMany, bool includeInactive = true)`: Same idea but instead of requiring one component, requires at least `howMany` components. With `includeInactive==true`, it also checks inactive components.
- `public static T[] RequireComponentsInChildren<T>(GameObject current, uint howMany, bool includeInactive = true)`: Same idea as immediately before but applied on the gameObject directly.
- `public static T AddComponent<T>(GameObject gameObject, Dictionary<string, object> data = null)`: Adds a component to the given gameObject, populating certain properties with data. Non-serializable fields are forbidden to be set. This method is useful in Editor mode, but also allowed in Runtime.
- `public static V EnsureInactive<V>(GameObject gameObject, Func<V> action)`: Performs an action (a function returning any type `V`) by setting the game object's `active` to false. After the function is executed, the game object's `active` returns to whatever was.
- `public static void SetObjectFieldValues(UnityEngine.Object target, Dictionary<string, object> data)`: Given a Unity object, it sets its serializable fields to certain values. Non-serializeble fields are forbidden to be set. This method is useful in Editor mode, but also allowed in Runtime.
- `public static HashSet<Type> GetDependencies(Component component)`: Returns the dependencies of a component (by looking at their `RequireComponent` attributes).
- `public static HashSet<Type> GetDependencies<T>()`: Returns the dependencies of a type of component  (by looking at their `RequireComponent` attributes).
- `public static Component[] SortByDependencies(Component[] components)`: Sorts all the components from the least dependent to the most dependent. Returns a sorted array.

#### `Behaviours.MissingParentException`

This one is triggered when a component is expected to have a parent but it doesn't have.

#### `Behaviours.MissingComponentInParentException`:

This one is triggered when a component is required in a parent object, but instead it's not there.

#### `Behaviours.MissingComponentInChildrenException`:

This one is triggered when a component is required among the children of this object, but it's not.

#### `Behaviours.CircularDependencyUnsupportedException`:

This one is triggered when there's a circular dependency among Behaviour objects.

#### `Behaviours.UnserializableFieldException`:

This one is triggered when trying to set, from serialization (when invoking `Behaviours.AddComponent` or `Behaviours.SetObjectFieldValues`), a field that is not a serializable one.