using System.Diagnostics;

namespace AutoBogus.Generators;

internal sealed class ReadOnlyDictionaryGenerator<TKey, TValue> : IAutoGenerator
    where TKey : notnull
{
    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        IAutoGenerator generator = new DictionaryGenerator<TKey, TValue>();

        var generateType = context.GenerateType;

        Debug.Assert(generateType != null, nameof(generateType) + " != null");

        if (generateType.IsInterface)
        {
            generateType = typeof(Dictionary<TKey, TValue>);
        }

        // Generate a standard dictionary and create the read only dictionary
        var items = generator.Generate(context) as IDictionary<TKey, TValue>;

        return Activator.CreateInstance(generateType, items)!;
    }
}
