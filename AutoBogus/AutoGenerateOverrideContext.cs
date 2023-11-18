using Bogus;

namespace AutoBogus;

/// <summary>
///     A class that provides context when overriding a generate request.
/// </summary>
public sealed class AutoGenerateOverrideContext
{
    internal AutoGenerateOverrideContext(AutoGenerateContext generateContext)
    {
        GenerateContext = generateContext;
    }

    /// <summary>
    ///     The instance generated during the override.
    /// </summary>
    public object? Instance { get; set; }

    /// <summary>
    ///     The type associated with the current generate request.
    /// </summary>
    public Type GenerateType => GenerateContext.GenerateType;

    /// <summary>
    ///     The name associated with the current generate request.
    /// </summary>
    public string? GenerateName => GenerateContext.GenerateName;

    /// <summary>
    ///     The underlying <see cref="Bogus.Faker" /> instance used to generate random values.
    /// </summary>
    public Faker Faker => GenerateContext.Faker;

    /// <summary>
    ///     The requested rule sets provided for the generate request.
    /// </summary>
    public IEnumerable<string> RuleSets => GenerateContext.RuleSets;

    internal AutoGenerateContext GenerateContext { get; }
}
