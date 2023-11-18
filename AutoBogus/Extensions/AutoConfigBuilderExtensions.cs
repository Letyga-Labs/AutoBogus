using System.Linq.Expressions;
using System.Reflection;

namespace AutoBogus;

/// <summary>
///     A class extending the config builder interfaces.
/// </summary>
public static class AutoConfigBuilderExtensions
{
    /// <summary>
    ///     Registers a binder type to use when generating values.
    /// </summary>
    /// <typeparam name="TBinder">The <see cref="IAutoBinder" /> type to use.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerDefaultConfigBuilder WithBinder<TBinder>(this IAutoFakerDefaultConfigBuilder builder)
        where TBinder : IAutoBinder, new()
    {
        ArgumentNullException.ThrowIfNull(builder);
        var binder = new TBinder();
        return builder.WithBinder(binder);
    }

    /// <summary>
    ///     Registers a binder type to use when generating values.
    /// </summary>
    /// <typeparam name="TBinder">The <see cref="IAutoBinder" /> type to use.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoGenerateConfigBuilder WithBinder<TBinder>(this IAutoGenerateConfigBuilder builder)
        where TBinder : IAutoBinder, new()
    {
        ArgumentNullException.ThrowIfNull(builder);
        var binder = new TBinder();
        return builder.WithBinder(binder);
    }

    /// <summary>
    ///     Registers a binder type to use when generating values.
    /// </summary>
    /// <typeparam name="TBinder">The <see cref="IAutoBinder" /> type to use.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerConfigBuilder WithBinder<TBinder>(this IAutoFakerConfigBuilder builder)
        where TBinder : IAutoBinder, new()
    {
        ArgumentNullException.ThrowIfNull(builder);
        var binder = new TBinder();
        return builder.WithBinder(binder);
    }

    /// <summary>
    ///     Registers a type to skip when generating values.
    /// </summary>
    /// <typeparam name="TType">The type to skip.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerDefaultConfigBuilder WithSkip<TType>(this IAutoFakerDefaultConfigBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var type = typeof(TType);
        return builder.WithSkip(type);
    }

    /// <summary>
    ///     Registers a type to skip when generating values.
    /// </summary>
    /// <typeparam name="TType">The type to skip.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoGenerateConfigBuilder WithSkip<TType>(this IAutoGenerateConfigBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var type = typeof(TType);
        return builder.WithSkip(type);
    }

    /// <summary>
    ///     Registers a type to skip when generating values.
    /// </summary>
    /// <typeparam name="TType">The type to skip.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerConfigBuilder WithSkip<TType>(this IAutoFakerConfigBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var type = typeof(TType);
        return builder.WithSkip(type);
    }

    /// <summary>
    ///     Registers a member to skip for a given type when generating values.
    /// </summary>
    /// <typeparam name="TType">The parent type containing the member.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <param name="member">The member to skip.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerDefaultConfigBuilder WithSkip<TType>(
        this IAutoFakerDefaultConfigBuilder builder,
        Expression<Func<TType, object?>>     member)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(member);

        var memberName = GetMemberName(member);
        return builder.WithSkip<TType>(memberName);
    }

    /// <summary>
    ///     Registers a member to skip for a given type when generating values.
    /// </summary>
    /// <typeparam name="TType">The parent type containing the member.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <param name="member">The member to skip.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoGenerateConfigBuilder WithSkip<TType>(
        this IAutoGenerateConfigBuilder builder,
        Expression<Func<TType, object?>> member)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(member);

        var memberName = GetMemberName(member);
        return builder.WithSkip<TType>(memberName);
    }

    /// <summary>
    ///     Registers a member to skip for a given type when generating values.
    /// </summary>
    /// <typeparam name="TType">The parent type containing the member.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <param name="member">The member to skip.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerConfigBuilder WithSkip<TType>(
        this IAutoFakerConfigBuilder    builder,
        Expression<Func<TType, object?>> member)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(member);

        var memberName = GetMemberName(member);
        return builder.WithSkip<TType>(memberName);
    }

    /// <summary>
    ///     Registers an override instance to use when generating values.
    /// </summary>
    /// <typeparam name="TType">The type of instance to override.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <param name="generator">A handler used to generate the override.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerDefaultConfigBuilder WithOverride<TType>(
        this IAutoFakerDefaultConfigBuilder      builder,
        Func<AutoGenerateOverrideContext, TType> generator)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var generatorOverride = new AutoGeneratorTypeOverride<TType>(generator);
        return builder.WithOverride(generatorOverride);
    }

    /// <summary>
    ///     Registers an override instance to use when generating values.
    /// </summary>
    /// <typeparam name="TType">The type of instance to override.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <param name="generator">A handler used to generate the override.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoGenerateConfigBuilder WithOverride<TType>(
        this IAutoGenerateConfigBuilder          builder,
        Func<AutoGenerateOverrideContext, TType> generator)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var generatorOverride = new AutoGeneratorTypeOverride<TType>(generator);
        return builder.WithOverride(generatorOverride);
    }

    /// <summary>
    ///     Registers an override instance to use when generating values.
    /// </summary>
    /// <typeparam name="TType">The type of instance to override.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <param name="generator">A handler used to generate the override.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerConfigBuilder WithOverride<TType>(
        this IAutoFakerConfigBuilder             builder,
        Func<AutoGenerateOverrideContext, TType> generator)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var generatorOverride = new AutoGeneratorTypeOverride<TType>(generator);
        return builder.WithOverride(generatorOverride);
    }

    /// <summary>
    ///     Registers an override instance to use when generating values.
    /// </summary>
    /// <typeparam name="TType">The type of instance to override.</typeparam>
    /// <typeparam name="TValue">The member type to override.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <param name="member">The member to override.</param>
    /// <param name="generator">A handler used to generate the override.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerDefaultConfigBuilder WithOverride<TType, TValue>(
        this IAutoFakerDefaultConfigBuilder       builder,
        Expression<Func<TType, object?>>           member,
        Func<AutoGenerateOverrideContext, TValue> generator)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(member);

        var memberName        = GetMemberName(member);
        var generatorOverride = new AutoGeneratorMemberOverride<TType, TValue>(memberName, generator);

        return builder.WithOverride(generatorOverride);
    }

    /// <summary>
    ///     Registers an override instance to use when generating values.
    /// </summary>
    /// <typeparam name="TType">The type of instance to override.</typeparam>
    /// <typeparam name="TValue">The member type to override.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <param name="member">The member to override.</param>
    /// <param name="generator">A handler used to generate the override.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoGenerateConfigBuilder WithOverride<TType, TValue>(
        this IAutoGenerateConfigBuilder           builder,
        Expression<Func<TType, object?>>           member,
        Func<AutoGenerateOverrideContext, TValue> generator)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(member);

        var memberName        = GetMemberName(member);
        var generatorOverride = new AutoGeneratorMemberOverride<TType, TValue>(memberName, generator);

        return builder.WithOverride(generatorOverride);
    }

    /// <summary>
    ///     Registers an override instance to use when generating values.
    /// </summary>
    /// <typeparam name="TType">The type of instance to override.</typeparam>
    /// <typeparam name="TValue">The member type to override.</typeparam>
    /// <param name="builder">The current configuration builder instance.</param>
    /// <param name="member">The member to override.</param>
    /// <param name="generator">A handler used to generate the override.</param>
    /// <returns>The given configuration builder instance.</returns>
    public static IAutoFakerConfigBuilder WithOverride<TType, TValue>(
        this IAutoFakerConfigBuilder              builder,
        Expression<Func<TType, object?>>           member,
        Func<AutoGenerateOverrideContext, TValue> generator)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(member);

        var memberName        = GetMemberName(member);
        var generatorOverride = new AutoGeneratorMemberOverride<TType, TValue>(memberName, generator);

        return builder.WithOverride(generatorOverride);
    }

    private static string GetMemberName<TType>(Expression<Func<TType, object?>> member)
    {
        MemberExpression expression;

        if (member.Body is UnaryExpression unary)
        {
            expression = (MemberExpression)unary.Operand;
        }
        else
        {
            expression = (MemberExpression)member.Body;
        }

        var memberInfo = expression.Member;

        if (memberInfo is FieldInfo || memberInfo is PropertyInfo)
        {
            return memberInfo.Name;
        }

        throw new InvalidOperationException($"Unexpected memberInfo type: {memberInfo}");
    }
}
