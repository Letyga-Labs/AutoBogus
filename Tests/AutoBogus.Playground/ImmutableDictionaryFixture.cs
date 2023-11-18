using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Xunit;

namespace AutoBogus.Playground;

public class ImmutableDictionaryFixture
{
    [Fact]
    public void Should_Populate_Object()
    {
        var obj = AutoFaker.Generate<Obj>();

        Assert.NotNull(obj);
        Assert.False(string.IsNullOrWhiteSpace(obj.SomeStringProperty));
        Assert.NotEmpty(obj.SomeReadOnlyDictionary);
        Assert.Null(obj.SomeImmutableDictionary);
    }

    private class Obj
    {
        public string                              SomeStringProperty      { get; set; } = null!;
        public ReadOnlyDictionary<string, string>  SomeReadOnlyDictionary  { get; set; } = null!;
        public ImmutableDictionary<string, string> SomeImmutableDictionary { get; set; } = null!;
    }
}
