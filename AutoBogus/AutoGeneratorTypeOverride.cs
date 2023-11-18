namespace AutoBogus;

internal sealed class AutoGeneratorTypeOverride<TType> : AutoGeneratorOverride
{
    private readonly Type _type;

    private readonly Func<AutoGenerateOverrideContext, TType> _generator;

    internal AutoGeneratorTypeOverride(Func<AutoGenerateOverrideContext, TType> generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        _type      = typeof(TType);
        _generator = generator;
    }

    public override bool CanOverride(AutoGenerateContext context)
    {
        return context.GenerateType == _type;
    }

    public override void Generate(AutoGenerateOverrideContext context)
    {
        context.Instance = _generator.Invoke(context);
    }
}
