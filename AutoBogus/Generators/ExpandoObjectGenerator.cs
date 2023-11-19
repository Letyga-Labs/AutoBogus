using System.Diagnostics;
using System.Dynamic;
using AutoBogus.Internal;

namespace AutoBogus.Generators;

internal sealed class ExpandoObjectGenerator : IAutoGenerator
{
    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        Debug.Assert(context.Instance != null, "context.Instance != null");

        var instance = context.Instance!;

        // Need to copy the target dictionary to avoid mutations during population
        var target = (IDictionary<string, object?>)instance;
        var source = new Dictionary<string, object?>(target);

        var properties = source.Where(pair => pair.Value != null);

        foreach (var property in properties)
        {
            // Configure the context
            var type = property.Value!.GetType();

            var value = Generation.Generate(
                context,
                parentType: context.GenerateType,
                generateType: type,
                generateName: property.Key,
                instance: type == typeof(ExpandoObject) ? property.Value : null);

            target[property.Key] = value;
        }

        // Reset the instance context
        context.Instance = instance;

        return instance;
    }
}
