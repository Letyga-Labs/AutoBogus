using NSubstitute;

namespace AutoBogus.NSubstitute;

/// <summary>
///     A class that enables NSubstitute binding for interface and abstract types.
/// </summary>
public class NSubstituteBinder : AutoBinder
{
    /// <summary>
    ///     Creates an instance of <typeparamref name="TType" />.
    /// </summary>
    /// <typeparam name="TType">The type of instance to create.</typeparam>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the generate request.</param>
    /// <returns>The created instance of <typeparamref name="TType" />.</returns>
    public override TType CreateUnpopulatedInstance<TType>(AutoGenerateContext context)
    {
        var type = typeof(TType);

        if (type.IsInterface || type.IsAbstract)
        {
            return (TType)Substitute.For(new[] { type, }, Array.Empty<object>());
        }

        return base.CreateUnpopulatedInstance<TType>(context);
    }
}
