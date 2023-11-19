using AutoBogus.Internal;

namespace AutoBogus;

/// <summary>
///     A class used to conveniently invoke generate requests.
/// </summary>
public sealed class AutoFaker : IAutoFaker
{
    internal static readonly AutoFakerConfig DefaultConfig = new();

    private AutoFaker(AutoFakerConfig config)
    {
        Config = config;
    }

    internal AutoFakerConfig Config { get; }

    /// <summary>
    ///     Configures all faker instances and generate requests.
    /// </summary>
    /// <param name="configure">A handler to build the default faker configuration.</param>
    public static void Configure(Action<IAutoFakerDefaultConfigBuilder>? configure)
    {
        var builder = new AutoFakerConfigBuilder(DefaultConfig);
        configure?.Invoke(builder);
    }

    /// <summary>
    ///     Creates a configured <see cref="IAutoFaker" /> instance.
    /// </summary>
    /// <param name="configure">A handler to build the faker configuration.</param>
    /// <returns>The configured <see cref="IAutoFaker" /> instance.</returns>
    public static IAutoFaker Create(Action<IAutoGenerateConfigBuilder>? configure = null)
    {
        var config  = new AutoFakerConfig(DefaultConfig);
        var builder = new AutoFakerConfigBuilder(config);
        configure?.Invoke(builder);

        return new AutoFaker(config);
    }

    /// <summary>
    ///     Generates an instance of type <typeparamref name="TType" />.
    /// </summary>
    /// <typeparam name="TType">The type of instance to generate.</typeparam>
    /// <param name="configure">A handler to build the generate request configuration.</param>
    /// <returns>The generated instance.</returns>
    public static TType Generate<TType>(Action<IAutoGenerateConfigBuilder>? configure = null)
    {
        var faker = Create(configure);
        return faker.Generate<TType>();
    }

    /// <summary>
    ///     Generates a collection of instances of type <typeparamref name="TType" />.
    /// </summary>
    /// <typeparam name="TType">The type of instance to generate.</typeparam>
    /// <param name="count">The number of instances to generate.</param>
    /// <param name="configure">A handler to build the generate request configuration.</param>
    /// <returns>The generated collection of instances.</returns>
    public static List<TType> Generate<TType>(int count, Action<IAutoGenerateConfigBuilder>? configure = null)
    {
        var faker = Create(configure);
        return faker.Generate<TType>(count);
    }

    TType IAutoFaker.Generate<TType>(Action<IAutoGenerateConfigBuilder>? configure)
    {
        var context = CreateContext(configure);
        return context.Generate<TType>();
    }

    List<TType> IAutoFaker.Generate<TType>(int count, Action<IAutoGenerateConfigBuilder>? configure)
    {
        var context = CreateContext(configure);
        return context.GenerateMany<TType>(count);
    }

    private AutoGenerateContext CreateContext(Action<IAutoGenerateConfigBuilder>? configure)
    {
        var config  = new AutoFakerConfig(Config);
        var builder = new AutoFakerConfigBuilder(config);
        configure?.Invoke(builder);

        return new AutoGenerateContext(config);
    }
}
