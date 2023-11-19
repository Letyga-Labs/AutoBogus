namespace AutoBogus.Internal;

/// <summary>
/// Contains all the isolated functionality for unpopulated values generation.
/// </summary>
internal static class Generation
{
    public static object Generate(
        AutoGenerateContext context,
        Type?               parentType,
        Type                generateType,
        string?             generateName,
        object?             instance = null)
    {
        context.ParentType   = parentType;
        context.GenerateType = generateType;
        context.GenerateName = generateName;
        if (instance != null)
        {
            context.Instance = instance;
        }

        var generator = GeneratorFactory.GetGenerator(context);
        var value     = generator.Generate(context);
        return value;
    }

    public static void GenerateMany<TType>(
        AutoGenerateContext context,
        List<TType>         items,
        bool                unique,
        int?                count    = null,
        int                 attempt  = 1,
        Func<TType>?        generate = null)
    {
        // Apply any defaults
        count    ??= context.Config.RepeatCount.Invoke(context);
        generate ??= context.Generate<TType>;

        // Generate a list of items
        var required = count - items.Count;

        for (var index = 0; index < required; index++)
        {
            var item = generate.Invoke();

            // Ensure the generated value is not null (which means the type couldn't be generated)
            if (item != null)
            {
                items.Add(item);
            }
        }

        if (!unique)
        {
            return;
        }

        // Remove any duplicates and generate more to match the required count
        var filtered = items.Distinct().ToList();

        if (filtered.Count >= count)
        {
            return;
        }

        // To maintain the items reference, clear and reapply the filtered list
        items.Clear();
        items.AddRange(filtered);

        // Only continue to generate more if the attempts threshold is not reached
        if (attempt < AutoFakerConfig.GenerateAttemptsThreshold)
        {
            GenerateMany(context, items, unique, count, attempt + 1, generate);
        }
    }
}
