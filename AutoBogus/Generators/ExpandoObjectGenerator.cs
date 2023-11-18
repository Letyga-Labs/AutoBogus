using System.Diagnostics;
using System.Dynamic;

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

            context.ParentType   = context.GenerateType;
            context.GenerateType = type;
            context.GenerateName = property.Key;

            context.Instance = type == typeof(ExpandoObject) ? property.Value : null;

            // Generate the property values
            var generator = AutoGeneratorFactory.GetGenerator(context);
            target[property.Key] = generator.Generate(context);
        }

        // Reset the instance context
        context.Instance = instance;

        return instance;
    }
}
