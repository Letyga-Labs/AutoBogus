namespace AutoBogus.Generators;

internal sealed class SetGenerator<TType>
    : IAutoGenerator
{
    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        ISet<TType> set;

        try
        {
            set = (ISet<TType>)Activator.CreateInstance(context.GenerateType);
        }
        catch
        {
            set = new HashSet<TType>();
        }

        var items = context.GenerateMany<TType>();

        foreach (var item in items)
        {
            set.Add(item);
        }

        return set;
    }
}
