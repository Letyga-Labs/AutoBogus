using Xunit;

namespace AutoBogus.Playground;

public class ParametersByRefFixture
{
    [Fact]
    public void Should_Create_Instance()
    {
        var instance = AutoFaker.Generate<TestClass>();
        Assert.NotNull(instance);
    }

    private class TestId
    {
        public string Value { get; set; } = null!;
    }

    private sealed class TestClass
    {
        public TestClass(in TestId id, out string value)
        {
            value = "OUT";

            Id    = id;
            Value = value;
        }

        public TestId Id    { get; }
        public string Value { get; }
    }
}
