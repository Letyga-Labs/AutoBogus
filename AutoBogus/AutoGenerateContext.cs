using System.Diagnostics;
using AutoBogus.Internal;
using Bogus;

namespace AutoBogus;

/// <summary>
///     A class that provides context for a generate request.
/// </summary>
[DebuggerDisplay(
    """
    Generation target:
    {GetGenerationTargetDebugView()}
    Generation config:
    {Config}
    """)]
public sealed class AutoGenerateContext
{
    internal AutoGenerateContext(AutoFakerConfig config)
        : this(config.FakerHub, config)
    {
    }

    internal AutoGenerateContext(Faker? faker, AutoFakerConfig config)
    {
        Faker  = faker ?? new Faker(config.Locale);
        Config = config;

        TypesStack = new Stack<Type>();

        RuleSets = Enumerable.Empty<string>();
    }

    /// <summary>
    ///     The parent type of the type associated with the current generate request.
    /// </summary>
    public Type? ParentType { get; internal set; }

    /// <summary>
    ///     The type associated with the current generate request.
    /// </summary>
    public Type GenerateType { get; internal set; } = null!;

    /// <summary>
    ///     The name associated with the current generate request.
    /// </summary>
    public string? GenerateName { get; internal set; }

    /// <summary>
    ///     The underlying <see cref="Bogus.Faker" /> instance used to generate random values.
    /// </summary>
    public Faker Faker { get; }

    /// <summary>
    ///     The requested rule sets provided for the generate request.
    /// </summary>
    public IEnumerable<string> RuleSets { get; internal set; }

    internal AutoFakerConfig Config { get; }

    internal Stack<Type> TypesStack { get; }

    internal object? Instance { get; set; }

    internal IAutoBinder Binder => Config.Binder;

    internal IEnumerable<AutoGeneratorOverride> Overrides => Config.Overrides;

    private string GetGenerationTargetDebugView()
    {
        return $"""
                    {ParentType?.ToString() ?? "<unknown parent type>"} (
                        {GenerateType} {GenerateName ?? "<unknown member name>"} instance =
                            {Instance ?? "<instance value is not set yet>"}
                    )
                 """;
    }
}
