using System.Reflection;

namespace AutoBogus;

internal sealed class AutoMember
{
    internal AutoMember(MemberInfo memberInfo)
    {
        Name = memberInfo.Name;

        switch (memberInfo)
        {
            case FieldInfo fieldInfo:
                Type       = fieldInfo.FieldType;
                IsReadOnly = !fieldInfo.IsPrivate && fieldInfo.IsInitOnly;
                Getter     = fieldInfo.GetValue;
                Setter     = fieldInfo.SetValue;
                break;

            case PropertyInfo propertyInfo:
                Type       = propertyInfo.PropertyType;
                IsReadOnly = !propertyInfo.CanWrite;
                Getter     = obj => propertyInfo.GetValue(obj, Array.Empty<object>());
                Setter     = (obj, value) => propertyInfo.SetValue(obj, value, Array.Empty<object>());
                break;

            default:
                throw new InvalidOperationException($"Unsupported memberInfo type: {memberInfo}");
        }
    }

    internal string Name       { get; }
    internal Type   Type       { get; }
    internal bool   IsReadOnly { get; }

    internal Func<object, object?>   Getter { get; }
    internal Action<object, object?> Setter { get; }
}
