namespace AutoBogus.Internal;

internal sealed class ClientCustomOverrideOfMemberGeneration<TType, TValue> : AutoGeneratorOverride
{
    private readonly Type   _type;
    private readonly string _memberName;

    private readonly Func<AutoGenerateOverrideContext, TValue> _generator;

    internal ClientCustomOverrideOfMemberGeneration(
        string                                    memberName,
        Func<AutoGenerateOverrideContext, TValue> generator)
    {
        if (string.IsNullOrWhiteSpace(memberName))
        {
            throw new ArgumentException("Value cannot be null or white space", nameof(memberName));
        }

        ArgumentNullException.ThrowIfNull(generator);

        _type       = typeof(TType);
        _memberName = memberName;
        _generator  = generator;
    }

    public override bool CanOverride(AutoGenerateContext context)
    {
        return context.ParentType == _type &&
               _memberName.Equals(context.GenerateName, StringComparison.OrdinalIgnoreCase);
    }

    public override void Generate(AutoGenerateOverrideContext context)
    {
        context.Instance = _generator.Invoke(context);
    }
}
