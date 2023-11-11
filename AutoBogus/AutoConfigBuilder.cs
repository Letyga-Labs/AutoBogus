using Bogus;

namespace AutoBogus;

internal sealed class AutoConfigBuilder :
    IAutoFakerDefaultConfigBuilder,
    IAutoGenerateConfigBuilder,
    IAutoFakerConfigBuilder
{
    internal AutoConfigBuilder(AutoConfig config)
    {
        Config = config;
    }

    internal AutoConfig Config { get; }

    internal object?[]? Args { get; private set; }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithLocale(string locale)
    {
        return WithLocale<IAutoFakerConfigBuilder>(locale, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithRepeatCount(int count)
    {
        return WithRepeatCount<IAutoFakerConfigBuilder>(_ => count, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithRepeatCount(
        Func<AutoGenerateContext, int> count)
    {
        return WithRepeatCount<IAutoFakerConfigBuilder>(count, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithDataTableRowCount(int count)
    {
        return WithDataTableRowCount<IAutoFakerConfigBuilder>(_ => count, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithDataTableRowCount(
        Func<AutoGenerateContext, int> count)
    {
        return WithDataTableRowCount<IAutoFakerConfigBuilder>(count, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithRecursiveDepth(int depth)
    {
        return WithRecursiveDepth<IAutoFakerConfigBuilder>(_ => depth, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithRecursiveDepth(
        Func<AutoGenerateContext, int> depth)
    {
        return WithRecursiveDepth<IAutoFakerConfigBuilder>(depth, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithTreeDepth(int? depth)
    {
        return WithTreeDepth<IAutoFakerConfigBuilder>(_ => depth, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithTreeDepth(
        Func<AutoGenerateContext, int?> depth)
    {
        return WithTreeDepth<IAutoFakerConfigBuilder>(depth, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithBinder(IAutoBinder binder)
    {
        return WithBinder<IAutoFakerConfigBuilder>(binder, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithFakerHub(Faker fakerHub)
    {
        return WithFakerHub<IAutoFakerConfigBuilder>(fakerHub, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithSkip(Type type)
    {
        return WithSkip<IAutoFakerConfigBuilder>(type, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithSkip(Type type, string memberName)
    {
        return WithSkip<IAutoFakerConfigBuilder>(type, memberName, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithSkip<TType>(string memberName)
    {
        return WithSkip<IAutoFakerConfigBuilder, TType>(memberName, this);
    }

    IAutoFakerConfigBuilder IAutoConfigBuilder<IAutoFakerConfigBuilder>.WithOverride(
        AutoGeneratorOverride generatorOverride)
    {
        return WithOverride<IAutoFakerConfigBuilder>(generatorOverride, this);
    }

    IAutoFakerConfigBuilder IAutoFakerConfigBuilder.WithArgs(params object[] args)
    {
        return WithArgs<IAutoFakerConfigBuilder>(args, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithLocale(string locale)
    {
        return WithLocale<IAutoFakerDefaultConfigBuilder>(locale, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithRepeatCount(int count)
    {
        return WithRepeatCount<IAutoFakerDefaultConfigBuilder>(_ => count, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithRepeatCount(
        Func<AutoGenerateContext, int> count)
    {
        return WithRepeatCount<IAutoFakerDefaultConfigBuilder>(count, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithDataTableRowCount(int count)
    {
        return WithDataTableRowCount<IAutoFakerDefaultConfigBuilder>(_ => count, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithDataTableRowCount(
        Func<AutoGenerateContext, int> count)
    {
        return WithDataTableRowCount<IAutoFakerDefaultConfigBuilder>(count, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithRecursiveDepth(int depth)
    {
        return WithRecursiveDepth<IAutoFakerDefaultConfigBuilder>(_ => depth, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithRecursiveDepth(
        Func<AutoGenerateContext, int> depth)
    {
        return WithRecursiveDepth<IAutoFakerDefaultConfigBuilder>(depth, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithTreeDepth(int? depth)
    {
        return WithTreeDepth<IAutoFakerDefaultConfigBuilder>(_ => depth, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithTreeDepth(
        Func<AutoGenerateContext, int?> depth)
    {
        return WithTreeDepth<IAutoFakerDefaultConfigBuilder>(depth, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithBinder(IAutoBinder binder)
    {
        return WithBinder<IAutoFakerDefaultConfigBuilder>(binder, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithFakerHub(Faker fakerHub)
    {
        return WithFakerHub<IAutoFakerDefaultConfigBuilder>(fakerHub, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithSkip(Type type)
    {
        return WithSkip<IAutoFakerDefaultConfigBuilder>(type, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithSkip(
        Type   type,
        string memberName)
    {
        return WithSkip<IAutoFakerDefaultConfigBuilder>(type, memberName, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithSkip<TType>(string memberName)
    {
        return WithSkip<IAutoFakerDefaultConfigBuilder, TType>(memberName, this);
    }

    IAutoFakerDefaultConfigBuilder IAutoConfigBuilder<IAutoFakerDefaultConfigBuilder>.WithOverride(
        AutoGeneratorOverride generatorOverride)
    {
        return WithOverride<IAutoFakerDefaultConfigBuilder>(generatorOverride, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithLocale(string locale)
    {
        return WithLocale<IAutoGenerateConfigBuilder>(locale, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithRepeatCount(int count)
    {
        return WithRepeatCount<IAutoGenerateConfigBuilder>(_ => count, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithRepeatCount(
        Func<AutoGenerateContext, int> count)
    {
        return WithRepeatCount<IAutoGenerateConfigBuilder>(count, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithDataTableRowCount(int count)
    {
        return WithDataTableRowCount<IAutoGenerateConfigBuilder>(_ => count, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithDataTableRowCount(
        Func<AutoGenerateContext, int> count)
    {
        return WithDataTableRowCount<IAutoGenerateConfigBuilder>(count, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithRecursiveDepth(int depth)
    {
        return WithRecursiveDepth<IAutoGenerateConfigBuilder>(_ => depth, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithRecursiveDepth(
        Func<AutoGenerateContext, int> depth)
    {
        return WithRecursiveDepth<IAutoGenerateConfigBuilder>(depth, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithTreeDepth(int? depth)
    {
        return WithTreeDepth<IAutoGenerateConfigBuilder>(_ => depth, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithTreeDepth(
        Func<AutoGenerateContext, int?> depth)
    {
        return WithTreeDepth<IAutoGenerateConfigBuilder>(depth, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithBinder(IAutoBinder binder)
    {
        return WithBinder<IAutoGenerateConfigBuilder>(binder, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithFakerHub(Faker fakerHub)
    {
        return WithFakerHub<IAutoGenerateConfigBuilder>(fakerHub, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithSkip(Type type)
    {
        return WithSkip<IAutoGenerateConfigBuilder>(type, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithSkip(Type type, string memberName)
    {
        return WithSkip<IAutoGenerateConfigBuilder>(type, memberName, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithSkip<TType>(string memberName)
    {
        return WithSkip<IAutoGenerateConfigBuilder, TType>(memberName, this);
    }

    IAutoGenerateConfigBuilder IAutoConfigBuilder<IAutoGenerateConfigBuilder>.WithOverride(
        AutoGeneratorOverride generatorOverride)
    {
        return WithOverride<IAutoGenerateConfigBuilder>(generatorOverride, this);
    }

    internal TBuilder WithLocale<TBuilder>(string? locale, TBuilder builder)
    {
        Config.Locale = locale ?? AutoConfig.DefaultLocale;
        return builder;
    }

    internal TBuilder WithRepeatCount<TBuilder>(Func<AutoGenerateContext, int>? count, TBuilder builder)
    {
        Config.RepeatCount = count ?? AutoConfig.DefaultRepeatCount;
        return builder;
    }

    internal TBuilder WithDataTableRowCount<TBuilder>(Func<AutoGenerateContext, int>? count, TBuilder builder)
    {
        Config.DataTableRowCount = count ?? AutoConfig.DefaultDataTableRowCount;
        return builder;
    }

    internal TBuilder WithRecursiveDepth<TBuilder>(Func<AutoGenerateContext, int>? depth, TBuilder builder)
    {
        Config.RecursiveDepth = depth ?? AutoConfig.DefaultRecursiveDepth;
        return builder;
    }

    internal TBuilder WithTreeDepth<TBuilder>(Func<AutoGenerateContext, int?>? depth, TBuilder builder)
    {
        Config.TreeDepth = depth ?? AutoConfig.DefaultTreeDepth;
        return builder;
    }

    internal TBuilder WithBinder<TBuilder>(IAutoBinder? binder, TBuilder builder)
    {
        Config.Binder = binder ?? new AutoBinder();
        return builder;
    }

    internal TBuilder WithFakerHub<TBuilder>(Faker? fakerHub, TBuilder builder)
    {
        Config.FakerHub = fakerHub;
        return builder;
    }

    internal TBuilder WithSkip<TBuilder>(Type type, TBuilder builder)
    {
        var existing = Config.SkipTypes.Any(it => it == type);
        if (!existing)
        {
            Config.SkipTypes.Add(type);
        }

        return builder;
    }

    internal TBuilder WithSkip<TBuilder>(Type type, string memberName, TBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            return builder;
        }

        var path     = $"{type.FullName}.{memberName}";
        var existing = Config.SkipPaths.Any(it => it == path);

        if (!existing)
        {
            Config.SkipPaths.Add(path);
        }

        return builder;
    }

    internal TBuilder WithSkip<TBuilder, TType>(string memberName, TBuilder builder)
    {
        return WithSkip(typeof(TType), memberName, builder);
    }

    internal TBuilder WithOverride<TBuilder>(AutoGeneratorOverride? generatorOverride, TBuilder builder)
    {
        if (generatorOverride == null)
        {
            return builder;
        }

        var existing = Config.Overrides.Any(it => it == generatorOverride);
        if (!existing)
        {
            Config.Overrides.Add(generatorOverride);
        }

        return builder;
    }

    internal TBuilder WithArgs<TBuilder>(object?[]? args, TBuilder builder)
    {
        Args = args;
        return builder;
    }
}
