using System.Diagnostics;
using AutoBogus.Internal;

namespace AutoBogus.Generators;

internal sealed class ListGenerator<TType> : IAutoGenerator
{
    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        IList<TType> list;

        try
        {
            Debug.Assert(context.GenerateType != null, "context.GenerateType != null");
            list = (IList<TType>)Activator.CreateInstance(context.GenerateType)!;
        }
        catch
        {
            list = new List<TType>();
        }

        var items = context.GenerateMany<TType>();

        foreach (var item in items)
        {
            list.Add(item);
        }

        return list;
    }
}
