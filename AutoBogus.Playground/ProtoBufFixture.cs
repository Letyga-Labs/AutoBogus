namespace AutoBogus.Playground;

public class ProtoBufFixture
{
    [Fact]
    public void Should_Populate_Field()
    {
        var protobuf = AutoFaker.Generate<Protobuf>();
        protobuf.Field.Should().NotBeEmpty();
        protobuf.MapField.Should().NotBeEmpty();
    }

    private class Protobuf
    {
        public Protobuf()
        {
            Field    = new RepeatedField<int>();
            MapField = new MapField<string, int>();
        }

        public RepeatedField<int>    Field    { get; }
        public MapField<string, int> MapField { get; }
    }
}
