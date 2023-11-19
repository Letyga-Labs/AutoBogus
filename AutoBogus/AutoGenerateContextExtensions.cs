using System.Diagnostics.CodeAnalysis;
using AutoBogus.Internal;

namespace AutoBogus;

/// <summary>
///     A class extending the <see cref="AutoGenerateContext" /> class.
/// </summary>
public static class AutoGenerateContextExtensions
{
    /// <summary>
    ///     Generates an instance of type <typeparamref name="TType" />.
    /// </summary>
    /// <typeparam name="TType">The instance type to generate.</typeparam>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the current generate request.</param>
    /// <returns>The generated instance.</returns>
    public static TType Generate<TType>(this AutoGenerateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = (TType)Generation.Generate(
            context,
            parentType: null,
            generateType: typeof(TType),
            generateName: null);

        return result;
    }

    /// <summary>
    ///     Generates a collection of instances of type <typeparamref name="TType" />.
    /// </summary>
    /// <typeparam name="TType">The instance type to generate.</typeparam>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the current generate request.</param>
    /// <param name="count">The number of instances to generate.</param>
    /// <returns>The generated collection of instances.</returns>
    public static List<TType> GenerateMany<TType>(this AutoGenerateContext context, int? count = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var items = new List<TType>();
        Generation.GenerateMany(context, items, unique: false, count);

        return items;
    }

    /// <summary>
    ///     Generates a collection of unique instances of type <typeparamref name="TType" />.
    /// </summary>
    /// <typeparam name="TType">The instance type to generate.</typeparam>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the current generate request.</param>
    /// <param name="count">The number of instances to generate.</param>
    /// <returns>The generated collection of unique instances.</returns>
    public static List<TType> GenerateUniqueMany<TType>(this AutoGenerateContext context, int? count = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        var items = new List<TType>();
        Generation.GenerateMany(context, items, unique: true, count);

        return items;
    }

    /// <summary>
    ///     Populates the provided instance with generated values.
    /// </summary>
    /// <typeparam name="TType">The type of instance to populate.</typeparam>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the current generate request.</param>
    /// <param name="instance">The instance to populate.</param>
    public static void Populate<TType>(this AutoGenerateContext context, [DisallowNull] TType instance)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(instance);

        context.Binder.PopulateInstance<TType>(instance, context);
    }
}
