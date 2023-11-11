using System.Reflection;
#if !NETSTANDARD1_3
using System.Dynamic;
#endif

namespace AutoBogus.Util;

internal static class ReflectionHelper
{
    internal static bool IsEnum(Type type)
    {
        var typeInfo = type.GetTypeInfo();
        return typeInfo.IsEnum;
    }

    internal static bool IsAbstract(Type type)
    {
        var typeInfo = type.GetTypeInfo();
        return typeInfo.IsAbstract;
    }

    internal static bool IsInterface(Type type)
    {
        var typeInfo = type.GetTypeInfo();
        return typeInfo.IsInterface;
    }

    internal static bool IsGenericType(Type type)
    {
        var typeInfo = type.GetTypeInfo();
        return typeInfo.IsGenericType;
    }

    internal static bool IsExpandoObject(Type? type)
    {
        return type == typeof(ExpandoObject);
    }

    internal static bool IsAssignableFrom(Type baseType, Type type)
    {
        var baseTypeInfo = baseType.GetTypeInfo();
        var typeInfo     = type.GetTypeInfo();

        return baseTypeInfo.IsAssignableFrom(typeInfo);
    }

    internal static bool IsField(MemberInfo member)
    {
        return member is FieldInfo;
    }

    internal static bool IsProperty(MemberInfo member)
    {
        return member is PropertyInfo;
    }

    internal static Type[] GetGenericArguments(Type type)
    {
        return type.GetGenericArguments();
    }

    internal static Type GetGenericTypeDefinition(Type type)
    {
        return type.GetGenericTypeDefinition();
    }

    internal static Type? GetGenericCollectionType(Type type)
    {
        var interfaces = type.GetInterfaces().Where(IsGenericType);

        if (IsInterface(type))
        {
            interfaces = interfaces.Concat(new[] { type, });
        }

        Type? dictionaryType         = null;
        Type? readOnlyDictionaryType = null;
        Type? listType               = null;
        Type? setType                = null;
        Type? collectionType         = null;
        Type? enumerableType         = null;

        foreach (var interfaceType in interfaces.Where(IsGenericType))
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

    internal static bool IsNullable(Type type)
    {
        return IsGenericTypeDefinition(typeof(Nullable<>), type);
    }

    private static bool IsGenericTypeDefinition(Type baseType, Type type)
    {
        if (!IsGenericType(type))
        {
            return false;
        }

        var definition = GetGenericTypeDefinition(type);

        // Do an assignable query first
        if (IsAssignableFrom(baseType, definition))
        {
            return true;
        }

        // If that don't work use the more complex interface checks
        return Array.Exists(type.GetInterfaces(), i => IsGenericTypeDefinition(baseType, i));
    }
}
