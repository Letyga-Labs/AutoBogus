namespace AutoBogus.Internal;

/// <summary>
/// Represents an actual low-level implementation of the <b>unpopulated</b> value generation.
/// Typically generates values of the concrete type of generic type family.
/// See <see cref="Generators"/> for implementation examples.
/// </summary>
internal interface IAutoGenerator
{
    object Generate(AutoGenerateContext context);
}
