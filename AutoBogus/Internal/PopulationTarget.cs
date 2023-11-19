using System.Reflection;

namespace AutoBogus.Internal;

/// <summary>
/// Represents type member which can be populated with some generated values.
/// It is, for example, some property or field which can be written to directly
/// or contains populatable via <c>Add(...)</c> method calls collection.
/// </summary>
internal sealed class PopulationTarget
{
    internal PopulationTarget(MemberInfo memberInfo)
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

    public override string ToString()
    {
        return $"{nameof(IsReadOnly)}: {IsReadOnly}, {nameof(Name)}: {Name}, {nameof(Type)}: {Type}";
    }
}
