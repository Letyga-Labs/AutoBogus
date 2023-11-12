using System.Reflection;
using AutoBogus.Util;
using Moq;

namespace AutoBogus.Moq;

/// <summary>
///     A class that enables Moq binding for interface and abstract types.
/// </summary>
public class MoqBinder
    : AutoBinder
{
    private static readonly MethodInfo _factory =
        typeof(Mock).GetMethod("Of", Array.Empty<Type>()) ??
        throw new InvalidOperationException(
            "Cannot find method `Of` of class `Mock`. Did an API changed unnoticeably?");

    /// <summary>
    ///     Creates an instance of <typeparamref name="TType" />.
    /// </summary>
    /// <typeparam name="TType">The type of instance to create.</typeparam>
    /// <param name="context">The <see cref="AutoGenerateContext" /> instance for the generate request.</param>
    /// <returns>The created instance of <typeparamref name="TType" />.</returns>
    public override TType CreateInstance<TType>(AutoGenerateContext context)
    {
        var type = typeof(TType);

        if (ReflectionHelper.IsInterface(type) || ReflectionHelper.IsAbstract(type))
        {
            // Take the cached factory method and make it generic based on the requested type
            // Because this method supports struct and class types, and Moq only supports class types we need to put this 'hack' into place
            var factory = _factory.MakeGenericMethod(type);
            return (TType)factory.Invoke(null, Array.Empty<object>())!;
        }

        return base.CreateInstance<TType>(context);
    }
}
