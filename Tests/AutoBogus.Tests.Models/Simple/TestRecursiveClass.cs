namespace AutoBogus.Tests.Models.Simple;

public sealed class TestRecursiveClass
{
    public TestRecursiveClass              Child    { get; set; } = null!;
    public IEnumerable<TestRecursiveClass> Children { get; set; } = null!;
    public TestRecursiveSubClass           Sub      { get; set; } = null!;
}
