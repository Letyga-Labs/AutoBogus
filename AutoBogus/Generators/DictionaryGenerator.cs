using System.Diagnostics;
using AutoBogus.Internal;

namespace AutoBogus.Generators;

internal sealed class DictionaryGenerator<TKey, TValue> : IAutoGenerator
    where TKey : notnull
{
    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        // Create an instance of a dictionary (public and non-public)
        IDictionary<TKey, TValue> items;
        try
        {
            Debug.Assert(context.GenerateType != null, "context.GenerateType != null");
            items = (IDictionary<TKey, TValue>)Activator.CreateInstance(context.GenerateType, true)!;
        }
        catch
        {
            items = new Dictionary<TKey, TValue>();
        }

        // Get a list of keys
        var keys = context.GenerateUniqueMany<TKey>();

        foreach (var key in keys)
        {
            // Get a matching value for the current key and add to the dictionary
            var value = context.Generate<TValue>();

            if (value != null)
            {
                items.Add(key, value);
            }
        }

        return items;
    }
}
