namespace AutoBogus.Errors;

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
