namespace AutoBogus.Tests.Models.Simple;

public sealed class OverrideClass
{
    public OverrideId Id   { get; } = new();
    public string?    Name { get; set; }

    public IEnumerable<int>? Amounts { get; set; }
}
