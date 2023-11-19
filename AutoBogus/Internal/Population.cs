using System.Collections;
using System.Reflection;
using AutoBogus.Errors;
using AutoBogus.Util;

namespace AutoBogus.Internal;

/// <summary>
/// Contains all the isolated functionality for members populating.
/// </summary>
internal static class Population
{
    public static void PopulateWithNewValue(PopulationTarget member, object memberHolder, object newValue)
    {
        try
        {
            if (!member.IsReadOnly)
            {
                member.Setter.Invoke(memberHolder, newValue);
            }
            else if (ReflectionHelper.IsDictionary(member.Type))
            {
                var newValues = newValue as IDictionary ?? throw new GeneratedValueCannotBeUsedForPopulationException(
                    $"""
                     The generated value for dictionaries stored in read only field must implement IDictionary
                     interface so that AutoFaker could enumerate through its items and add them to the populating target.
                     The generated value was {newValue} of type {newValue.GetType()}.
                     """);

                PopulateDictionary(member, memberHolder, newValues);
            }
            else if (ReflectionHelper.IsCollection(member.Type))
            {
                var newValues = newValue as ICollection ?? throw new GeneratedValueCannotBeUsedForPopulationException(
                    $"""
                     The generated value for dictionaries stored in read only field must implement ICollection
                     interface so that AutoFaker could enumerate through its items and add them to the populating target.
                     The generated value was {newValue} of type {newValue.GetType()}.
                     """);

                PopulateCollection(member, memberHolder, newValues);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void PopulateDictionary(PopulationTarget member, object parent, IDictionary newValues)
    {
        var instance = member.Getter(parent);
        switch (instance)
        {
            case null:
                return;

            // fast case
            case IDictionary dictionary:
            {
                foreach (var key in newValues.Keys)
                {
                    dictionary.Add(key, newValues[key]);
                }

                break;
            }

            // slow case
            default:
            {
                var addMethod = GetAddMethod(member);

                if (addMethod == null)
                {
                    return;
                }

                foreach (var key in newValues.Keys)
                {
                    addMethod.Invoke(instance, new[] { key, newValues[key], });
                }

                break;
            }
        }
    }

    private static void PopulateCollection(PopulationTarget member, object parent, ICollection newValues)
    {
        var instance = member.Getter(parent);
        switch (instance)
        {
            case null:
                return;

            // fast case
            case IList list:
            {
                foreach (var item in newValues)
                {
                    list.Add(item);
                }

                break;
            }

            // slow case
            default:
            {
                var addMethod = GetAddMethod(member);

                if (addMethod == null)
                {
                    return;
                }

                foreach (var item in newValues)
                {
                    addMethod.Invoke(instance, new[] { item, });
                }

                break;
            }
        }
    }

    private static MethodInfo? GetAddMethod(PopulationTarget member)
    {
        var argTypes = new[] { typeof(object), };

        if (member.Type.IsGenericType)
        {
            var generics = member.Type.GetGenericArguments();
            argTypes = generics.ToArray();
        }

        var addMethod = ReflectionHelper.FindMethod(member.Type, "Add", argTypes);
        return addMethod;
    }
}
