namespace AutoBogus.Errors;

/// <summary>
/// States that the implementation could not find any appropriate constructor
/// in order to create an unpopulated instance of the type.
/// </summary>
public sealed class CouldNotFindAppropriateConstructorException : Exception
{
    public CouldNotFindAppropriateConstructorException()
    {
    }

    public CouldNotFindAppropriateConstructorException(string message)
        : base(message)
    {
    }

    public CouldNotFindAppropriateConstructorException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
