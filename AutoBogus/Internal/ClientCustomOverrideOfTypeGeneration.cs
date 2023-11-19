namespace AutoBogus.Internal;

internal sealed class ClientCustomOverrideOfTypeGeneration<TType> : AutoGeneratorOverride
{
    private readonly Type _type;

    private readonly Func<AutoGenerateOverrideContext, TType> _generator;

    internal ClientCustomOverrideOfTypeGeneration(Func<AutoGenerateOverrideContext, TType> generator)
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
