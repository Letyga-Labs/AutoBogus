using System.Reflection;

namespace AutoBogus.Util;

/// <summary>
/// A set of various general reflection utilities empowering AutoFaker.
/// </summary>
internal static class ReflectionHelper
{
    internal static Type? GetGenericCollectionType(Type type)
    {
        var interfaces = type.GetInterfaces().Where(type1 => type1.IsGenericType);

        if (type.IsInterface)
        {
            interfaces = interfaces.Concat(new[] { type, });
        }

        Type? dictionaryType         = null;
        Type? readOnlyDictionaryType = null;
        Type? listType               = null;
        Type? setType                = null;
        Type? collectionType         = null;
        Type? enumerableType         = null;

        foreach (var interfaceType in interfaces.Where(type1 => type1.IsGenericType))
        {
            if (IsDictionary(interfaceType))
            {
                dictionaryType = interfaceType;
            }

            if (IsReadOnlyDictionary(interfaceType))
            {
                readOnlyDictionaryType = interfaceType;
            }

            if (IsList(interfaceType))
            {
                listType = interfaceType;
            }

            if (IsSet(interfaceType))
            {
                setType = interfaceType;
            }

            if (IsCollection(interfaceType))
            {
                collectionType = interfaceType;
            }

            if (IsEnumerable(interfaceType))
            {
                enumerableType = interfaceType;
            }
        }

        if (dictionaryType != null && readOnlyDictionaryType != null && IsReadOnlyDictionary(type))
        {
            dictionaryType = null;
        }

        return dictionaryType ?? readOnlyDictionaryType ?? listType ?? setType ?? collectionType ?? enumerableType;
    }

    internal static bool IsDictionary(Type type)
    {
        var baseType = typeof(IDictionary<,>);
        return IsGenericTypeDefinition(baseType, type);
    }

    internal static bool IsReadOnlyDictionary(Type type)
    {
        var baseType = typeof(IReadOnlyDictionary<,>);

        if (!IsGenericTypeDefinition(baseType, type))
        {
            return false;
        }

        // Read only dictionaries don't have an Add() method
        var methods = type
            .GetMethods()
            .Where(m => m.Name.Equals("Add", StringComparison.Ordinal));

        return !methods.Any();
    }

    internal static bool IsSet(Type type)
    {
        var baseType = typeof(ISet<>);
        return IsGenericTypeDefinition(baseType, type);
    }

    internal static bool IsList(Type type)
    {
        var baseType = typeof(IList<>);
        return IsGenericTypeDefinition(baseType, type);
    }

    internal static bool IsCollection(Type type)
    {
        var baseType = typeof(ICollection<>);
        return IsGenericTypeDefinition(baseType, type);
    }

    internal static bool IsEnumerable(Type type)
    {
        var baseType = typeof(IEnumerable<>);
        return IsGenericTypeDefinition(baseType, type);
    }

    internal static bool IsValueNullable(Type type)
    {
        return IsGenericTypeDefinition(typeof(Nullable<>), type);
    }

    internal static MethodInfo? FindMethod(Type type, string methodName, Type[] parameterTypes)
    {
        // First try directly on the type
        var method = type.GetMethod(methodName, parameterTypes);

        if (method != null)
        {
            return method;
        }

        // Then traverse the type interfaces
        foreach (var baseInterface in type.GetInterfaces())
        {
            var result = FindMethod(baseInterface, methodName, parameterTypes);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    ///     Find the first constructor that matches the passed generic definition.
    /// </summary>
    /// <returns>Constructor if found, null otherwise.</returns>
    internal static ConstructorInfo? ResolveTypedConstructor(Type type, IEnumerable<ConstructorInfo> constructors)
    {
        var result = constructors.SingleOrDefault(constructor =>
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length != 1)
            {
                return false;
            }

            var paramType = parameters[0].ParameterType;

            return paramType.IsGenericType && paramType.GetGenericTypeDefinition() == type;
        });

        return result;
    }

    private static bool IsGenericTypeDefinition(Type baseType, Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var definition = type.GetGenericTypeDefinition();

        // Do an assignable query first
        if (baseType.IsAssignableFrom(definition))
        {
            return true;
        }

        // If that don't work use the more complex interface checks
        return Array.Exists(type.GetInterfaces(), i => IsGenericTypeDefinition(baseType, i));
    }
}
