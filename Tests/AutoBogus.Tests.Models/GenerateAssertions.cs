using System.Collections;
using System.Globalization;
using System.Net;
using System.Reflection;
using AutoBogus.Util;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using static System.Globalization.CultureInfo;

namespace AutoBogus.Tests.Models;

public sealed class GenerateAssertions
    : ReferenceTypeAssertions<object, GenerateAssertions>
{
    private readonly IDictionary<Func<Type, bool>, Func<string?, Type, object?, string?>> _assertions =
        new Dictionary<Func<Type, bool>, Func<string?, Type, object?, string?>>();

    private readonly MethodInfo _defaultValueFactory;

    internal GenerateAssertions(object subject)
        : base(subject)
    {
        var type = GetType();

        _defaultValueFactory = type.GetMethod("GetDefaultValue", BindingFlags.Instance | BindingFlags.NonPublic)!;

        // Add the assertions to type mappings
        _assertions.Add(IsBool,           AssertBool);
        _assertions.Add(IsByte,           AssertByte);
        _assertions.Add(IsChar,           AssertChar);
        _assertions.Add(IsDateTime,       AssertDateTime);
        _assertions.Add(IsDateTimeOffset, AssertDateTimeOffset);
        _assertions.Add(IsDecimal,        AssertDecimal);
        _assertions.Add(IsDouble,         AssertDouble);
        _assertions.Add(IsFloat,          AssertFloat);
        _assertions.Add(IsGuid,           AssertGuid);
        _assertions.Add(IsInt,            AssertInt);
        _assertions.Add(IsIpAddress,      AssertIpAddress);
        _assertions.Add(IsLong,           AssertLong);
        _assertions.Add(IsSByte,          AssertSByte);
        _assertions.Add(IsShort,          AssertShort);
        _assertions.Add(IsString,         AssertString);
        _assertions.Add(IsUInt,           AssertUInt);
        _assertions.Add(IsULong,          AssertULong);
        _assertions.Add(IsUri,            AssertUri);
        _assertions.Add(IsUShort,         AssertUShort);

        _assertions.Add(IsArray,      AssertArray);
        _assertions.Add(IsEnum,       AssertEnum);
        _assertions.Add(IsDictionary, AssertDictionary);
        _assertions.Add(IsEnumerable, AssertEnumerable);
        _assertions.Add(IsNullable,   AssertNullable);
    }

    protected override string Identifier => "Generate";

    private IAssertionScope? Scope { get; set; }

    public AndConstraint<object> BeGenerated()
    {
        var type      = Subject.GetType();
        var assertion = GetAssertion(type);

        Scope = Execute.Assertion;

        // Assert the value and output any fail messages
        var message = assertion.Invoke(null, type, Subject);

        Scope = Scope
            .ForCondition(message == null)
            .FailWith(message)
            .Then;

        return new AndConstraint<object>(Subject);
    }

    public AndConstraint<object> BeGeneratedWithMocks()
    {
        // Ensure the mocked objects are asserted as not null
        _assertions.Add(IsInterface, AssertMock);
        _assertions.Add(IsAbstract,  AssertMock);

        return AssertSubject();
    }

    public AndConstraint<object> BeGeneratedWithoutMocks()
    {
        // Ensure the mocked objects are asserted as null
        _assertions.Add(IsInterface, AssertNull);
        _assertions.Add(IsAbstract,  AssertNull);

        return AssertSubject();
    }

    public AndConstraint<object> NotBeGenerated()
    {
        var type        = Subject.GetType();
        var memberInfos = GetMemberInfos(type);

        Scope = Execute.Assertion;

        foreach (var memberInfo in memberInfos)
        {
            AssertDefaultValue(memberInfo);
        }

        return new AndConstraint<object>(Subject);
    }

    private static bool IsBool(Type type)
    {
        return type == typeof(bool);
    }

    private static bool IsByte(Type type)
    {
        return type == typeof(byte);
    }

    private static bool IsChar(Type type)
    {
        return type == typeof(char);
    }

    private static bool IsDateTime(Type type)
    {
        return type == typeof(DateTime);
    }

    private static bool IsDateTimeOffset(Type type)
    {
        return type == typeof(DateTimeOffset);
    }

    private static bool IsDecimal(Type type)
    {
        return type == typeof(decimal);
    }

    private static bool IsDouble(Type type)
    {
        return type == typeof(double);
    }

    private static bool IsFloat(Type type)
    {
        return type == typeof(float);
    }

    private static bool IsGuid(Type type)
    {
        return type == typeof(Guid);
    }

    private static bool IsInt(Type type)
    {
        return type == typeof(int);
    }

    private static bool IsIpAddress(Type type)
    {
        return type == typeof(IPAddress);
    }

    private static bool IsLong(Type type)
    {
        return type == typeof(long);
    }

    private static bool IsSByte(Type type)
    {
        return type == typeof(sbyte);
    }

    private static bool IsShort(Type type)
    {
        return type == typeof(short);
    }

    private static bool IsString(Type type)
    {
        return type == typeof(string);
    }

    private static bool IsUInt(Type type)
    {
        return type == typeof(uint);
    }

    private static bool IsULong(Type type)
    {
        return type == typeof(ulong);
    }

    private static bool IsUri(Type type)
    {
        return type == typeof(Uri);
    }

    private static bool IsUShort(Type type)
    {
        return type == typeof(ushort);
    }

    private static bool IsArray(Type type)
    {
        return type.IsArray;
    }

    private static bool IsEnum(Type type)
    {
        return ReflectionHelper.IsEnum(type);
    }

    private static bool IsDictionary(Type type)
    {
        return IsType(type, typeof(IDictionary<,>));
    }

    private static bool IsEnumerable(Type type)
    {
        return IsType(type, typeof(IEnumerable<>));
    }

    private static bool IsNullable(Type type)
    {
        return ReflectionHelper.IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private static bool IsAbstract(Type type)
    {
        return ReflectionHelper.IsAbstract(type);
    }

    private static bool IsInterface(Type type)
    {
        return ReflectionHelper.IsInterface(type);
    }

    private static string? AssertBool(string? path, Type type, object? value)
    {
        return value != null && bool.TryParse(value.ToString(), out var result)
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertByte(string? path, Type type, object? value)
    {
        return value != null && byte.TryParse(value.ToString(), out var result)
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertChar(string? path, Type type, object? value)
    {
        return value != null && char.TryParse(value.ToString(), out var result) && result != default(char)
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertDateTime(string? path, Type type, object? value)
    {
        var isParsedSuccessfuly =
            value != null
            && DateTime.TryParse(value.ToString(), CurrentCulture, DateTimeStyles.AssumeUniversal, out var result)
            && result != default;

        return isParsedSuccessfuly ? null : GetAssertionMessage(path, type, value);
    }

    private static string? AssertDateTimeOffset(string? path, Type type, object? value)
    {
        var isParsedSuccessfuly =
            value != null
            && DateTimeOffset.TryParse(value.ToString(), CurrentCulture, DateTimeStyles.AssumeUniversal, out var result)
            && result != default;

        return isParsedSuccessfuly ? null : GetAssertionMessage(path, type, value);
    }

    private static string? AssertDecimal(string? path, Type type, object? value)
    {
        return value != null && decimal.TryParse(value.ToString(), out var result) && result != default
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertDouble(string? path, Type type, object? value)
    {
        return value != null && double.TryParse(value.ToString(), out var result) && result != default
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertFloat(string? path, Type type, object? value)
    {
        return value != null && float.TryParse(value.ToString(), out var result) && result != default
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertGuid(string? path, Type type, object? value)
    {
        return value != null && Guid.TryParse(value.ToString(), out var result) && result != Guid.Empty
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertInt(string? path, Type type, object? value)
    {
        return value != null && int.TryParse(value.ToString(), out var result) && result != default
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertIpAddress(string? path, Type type, object? value)
    {
        return value != null && IPAddress.TryParse(value.ToString(), out var result)
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertLong(string? path, Type type, object? value)
    {
        return value != null && long.TryParse(value.ToString(), out var result) && result != default
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertSByte(string? path, Type type, object? value)
    {
        return value != null && sbyte.TryParse(value.ToString(), out var result)
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertShort(string? path, Type type, object? value)
    {
        return value != null && short.TryParse(value.ToString(), out var result) && result != default(short)
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertUInt(string? path, Type type, object? value)
    {
        return value != null && uint.TryParse(value.ToString(), out var result) && result != default
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertULong(string? path, Type type, object? value)
    {
        return value != null && ulong.TryParse(value.ToString(), out var result) && result != default
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertUri(string? path, Type type, object? value)
    {
        return value != null && Uri.IsWellFormedUriString(value.ToString(), UriKind.RelativeOrAbsolute)
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertUShort(string? path, Type type, object? value)
    {
        return value != null && ushort.TryParse(value.ToString(), out var result) && result != default(ushort)
            ? null
            : GetAssertionMessage(path, type, value);
    }

    private static string? AssertEnum(string? path, Type type, object? value)
    {
        return value != null && Enum.IsDefined(type, value) ? null : GetAssertionMessage(path, type, value);
    }

    private static string? AssertNull(string? path, Type type, object? value)
    {
        return value == null ? null : $"Expected value to be null for '{path}'.";
    }

    private static string? AssertString(string? path, Type type, object? value)
    {
        var str = value?.ToString();
        return string.IsNullOrWhiteSpace(str) ? GetAssertionMessage(path, type, value) : null;
    }

    private static string GetAssertionMessage(string? path, Type type, object? value)
    {
        return string.IsNullOrWhiteSpace(path)
            ? $"Excepted a value of type '{type.FullName}' -> {value ?? "?"}."
            : $"Excepted a value of type '{type.FullName}' for '{path}' -> {value ?? "?"}.";
    }

    private static bool IsType(Type type, Type baseType)
    {
        // We may need to do some generics magic to compare the types
        if (!ReflectionHelper.IsGenericType(type) || !ReflectionHelper.IsGenericType(baseType))
        {
            return baseType.IsAssignableFrom(type);
        }

        var types     = type.GetGenericArguments();
        var baseTypes = baseType.GetGenericArguments();

        if (types.Length == baseTypes.Length)
        {
            baseType = baseType.MakeGenericType(types);
        }

        return baseType.IsAssignableFrom(type);
    }

    private static void ExtractMemberInfo(
        MemberInfo                 member,
        out Type                   memberType,
        out Func<object?, object?> memberGetter)
    {
        // Extract the member type and getter action
        if (ReflectionHelper.IsField(member))
        {
            var fieldInfo = (FieldInfo)member;
            memberType   = fieldInfo.FieldType;
            memberGetter = fieldInfo.GetValue;
        }
        else if (ReflectionHelper.IsProperty(member))
        {
            var propertyInfo = (PropertyInfo)member;
            memberType   = propertyInfo.PropertyType;
            memberGetter = propertyInfo.GetValue;
        }
        else
        {
            throw new InvalidOperationException($"Unexpected member: {member}");
        }
    }

    private AndConstraint<object> AssertSubject()
    {
        var type      = Subject.GetType();
        var assertion = GetAssertion(type);

        Scope = Execute.Assertion;

        assertion.Invoke(type.Name, type, Subject);

        return new AndConstraint<object>(Subject);
    }

    private string? AssertType(string? path, Type type, object? instance)
    {
        // Iterate the members for the instance and assert their values
        var members = GetMemberInfos(type);

        foreach (var member in members)
        {
            AssertMember(path, member, instance);
        }

        return null;
    }

    private void AssertDefaultValue(MemberInfo memberInfo)
    {
        ExtractMemberInfo(memberInfo, out var memberType, out var memberGetter);

        // Resolve the default value for the current member type and check it matches
        var factory      = _defaultValueFactory.MakeGenericMethod(memberType);
        var defaultValue = factory.Invoke(this, Array.Empty<object>());
        var value        = memberGetter.Invoke(Subject);
        var equal        = value == null && defaultValue == null;

        if (!equal)
        {
            // Ensure Equals() is called on a non-null instance
            equal = value?.Equals(defaultValue) ?? defaultValue!.Equals(value);
        }

        Scope = Scope!
            .ForCondition(equal)
            .FailWith($"Expected a default '{memberType.FullName}' value for '{memberInfo.Name}'.")
            .Then;
    }

    private string? AssertNullable(string? path, Type type, object? value)
    {
        var genericType = type.GenericTypeArguments.Single();
        var assertion   = GetAssertion(genericType);

        return assertion.Invoke(path, genericType, value);
    }

    private string? AssertMock(string? path, Type type, object? value)
    {
        if (value == null)
        {
            return $"Excepted value to not be null for '{path}'.";
        }

        // Assert via assignment rather than explicit checks (the actual instance could be a sub class)
        var valueType = value.GetType();
        return type.IsAssignableFrom(valueType) ? null : GetAssertionMessage(path, type, value);
    }

    private string? AssertArray(string? path, Type type, object? value)
    {
        var itemType = type.GetElementType() ?? throw new InvalidOperationException();
        return AssertItems(path, itemType, value as Array);
    }

    private string? AssertDictionary(string? path, Type type, object? value)
    {
        var genericTypes = type.GetGenericArguments();
        var keyType      = genericTypes[0];
        var valueType    = genericTypes[1];
        var dictionary   = value as IDictionary;

        if (dictionary == null)
        {
            return $"Excepted value to not be null for '{path}'.";
        }

        // Check the keys and values individually
        var keysMessage = AssertItems(path, keyType, dictionary.Keys, "keys", ".Key");

        if (keysMessage == null)
        {
            return AssertItems(path, valueType, dictionary.Values, "values", ".Value");
        }

        return keysMessage;
    }

    private string? AssertEnumerable(string? path, Type type, object? value)
    {
        var genericTypes = type.GetGenericArguments();
        var itemType     = genericTypes.Single();

        return AssertItems(path, itemType, value as IEnumerable);
    }

    private string? AssertItems(
        string?      path,
        Type         type,
        IEnumerable? items,
        string?      elementType = null,
        string?      suffix      = null)
    {
        // Check the list of items is not null
        if (items == null)
        {
            return $"Excepted value to not be null for '{path}'.";
        }

        // Check the count state of the items
        var count      = 0;
        var enumerator = items.GetEnumerator();

        while (enumerator.MoveNext())
        {
            count++;
        }

        if (count > 0)
        {
            // If we have any items, check each of them
            var index     = 0;
            var assertion = GetAssertion(type);

            foreach (var item in items)
            {
                var element = string.Format(CurrentCulture, "{0}[{1}]{2}", path, index++, suffix);
                var message = assertion.Invoke(element, type, item);

                if (message != null)
                {
                    return message;
                }
            }
        }
        else
        {
            // Otherwise ensure we are not dealing with interface or abstract class
            // These types will result in an empty list by default because they cannot be generated
            if (!ReflectionHelper.IsInterface(type) && !ReflectionHelper.IsAbstract(type))
            {
                elementType ??= "value";
                return $"Excepted {elementType} to not be empty for '{path}'.";
            }
        }

        return null;
    }

    private void AssertMember(string? path, MemberInfo memberInfo, object? instance)
    {
        ExtractMemberInfo(memberInfo, out var memberType, out var memberGetter);

        // Create a trace path for the current member
        path = string.Concat(path, ".", memberInfo.Name);

        // Resolve the assertion and value for the member type
        var value     = memberGetter.Invoke(instance);
        var assertion = GetAssertion(memberType);
        var message   = assertion.Invoke(path, memberType, value);

        // Register an assertion for each member
        Scope = Scope!
            .ForCondition(message == null)
            .FailWith(message)
            .Then;
    }

    private object? GetDefaultValue<TType>()
    {
        // This method is used via reflection above
        return default(TType);
    }

    private Func<string?, Type, object?, string?> GetAssertion(Type type)
    {
        var assertion = _assertions.Keys
            .Where(k => k.Invoke(type))
            .Select(k => _assertions[k])
            .FirstOrDefault();

        return assertion ?? AssertType;
    }

    private IEnumerable<MemberInfo> GetMemberInfos(Type type)
    {
        return type
            .GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(m => ReflectionHelper.IsField(m) || ReflectionHelper.IsProperty(m));
    }
}
