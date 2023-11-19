namespace AutoBogus.Errors;

/// <summary>
/// Represents an internal AutoFaker error. Please fill an issue if you will encounter such.
/// States that generator created a value that implementation somehow cannot use to populate some member.
/// </summary>
public sealed class GeneratedValueCannotBeUsedForPopulationException : Exception
{
    public GeneratedValueCannotBeUsedForPopulationException()
    {
    }

    public GeneratedValueCannotBeUsedForPopulationException(string message)
        : base(message)
    {
    }

    public GeneratedValueCannotBeUsedForPopulationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
