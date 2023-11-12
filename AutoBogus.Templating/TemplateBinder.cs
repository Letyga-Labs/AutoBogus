namespace AutoBogus.Templating;

/// <summary>
///     Used to configure the Templator.
/// </summary>
public class TemplateBinder : AutoBinder
{
    internal List<Func<Type, string, (bool Handled, object? Result)>> TypeConverters { get; } = new();

    internal bool TreatMissingAsNull { get; set; } = true;

    internal string? PropertyNameSpaceDelimiter { get; set; }

    /// <summary>
    ///     Will set missing values as empty string rather than null.
    /// </summary>
    /// <returns>The binder being configured.</returns>
    public TemplateBinder TreatMissingAsEmpty()
    {
        TreatMissingAsNull = false;
        return this;
    }

    /// <summary>
    ///     Add a type converter.
    /// </summary>
    /// <returns>The binder being configured.</returns>
    public TemplateBinder SetTypeConverter(Func<Type, string, (bool Handled, object? Result)> typeConverter)
    {
        TypeConverters.Add(typeConverter);
        return this;
    }

    /// <summary>
    ///     If the field name in the header contains a space it will translate to the property name by inserting this
    ///     delimiter.
    /// </summary>
    /// <returns>The binder being configured.</returns>
    public TemplateBinder SetPropertyNameSpaceDelimiter(string value)
    {
        PropertyNameSpaceDelimiter = value;
        return this;
    }
}
