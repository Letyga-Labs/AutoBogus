using Xunit;

namespace AutoBogus.Playground;

public class OverridesFixture
{
    [Fact]
    public void Should_Override()
    {
        var name = AutoFaker.Generate<string>();
        var ex   = new InvalidOperationException();
        var obj = AutoFaker.Generate<Obj>(builder =>
        {
            builder
                .WithOverride<Obj, string>(model => model.Name, _ => name)
                .WithOverride<Exception>(_ => ex);
        });

        Assert.Equal(name, obj.Name);
        Assert.Equal(ex,   obj.Exception);
    }

    private sealed class Obj
    {
        public int       Id        { get; set; }
        public string    Name      { get; set; } = null!;
        public Exception Exception { get; set; } = null!;
    }
}
