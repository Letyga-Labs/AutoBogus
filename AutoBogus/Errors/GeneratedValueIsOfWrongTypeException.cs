namespace AutoBogus.Errors;

public sealed class GeneratedValueIsOfWrongTypeException : Exception
{
    public GeneratedValueIsOfWrongTypeException()
    {
    }

    public GeneratedValueIsOfWrongTypeException(string message)
        : base(message)
    {
    }

    public GeneratedValueIsOfWrongTypeException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
