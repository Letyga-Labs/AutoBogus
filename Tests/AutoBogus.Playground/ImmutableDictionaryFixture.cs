using System.Collections.Immutable;
using System.Collections.ObjectModel;
using FluentAssertions;
using Xunit;

namespace AutoBogus.Playground;

public class ImmutableDictionaryFixture
{
    [Fact]
    public void Should_Populate_Object()
    {
        var obj = AutoFaker.Generate<Obj>();

        obj.Should().NotBeNull();
        obj.SomeStringProperty.Should().NotBeNullOrWhiteSpace();
        obj.SomeReadOnlyDictionary.Should().NotBeEmpty();
        obj.SomeImmutableDictionary.Should().BeNull();
    }

    private class Obj
    {
        public string                              SomeStringProperty      { get; set; } = null!;
        public ReadOnlyDictionary<string, string>  SomeReadOnlyDictionary  { get; set; } = null!;
        public ImmutableDictionary<string, string> SomeImmutableDictionary { get; set; } = null!;
    }
}
