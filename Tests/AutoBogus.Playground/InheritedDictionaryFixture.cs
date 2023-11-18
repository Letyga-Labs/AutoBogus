using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace AutoBogus.Playground;

public class InheritedDictionaryFixture
{
    [Fact]
    public void Should_Populate_Object()
    {
        var obj = AutoFaker.Generate<Obj>();

        Assert.NotNull(obj);
        Assert.NotNull(obj.Properties);
    }

    [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
    public class Obj
    {
        public Guid       Id         { get; set; }
        public Properties Properties { get; set; } = null!;
    }

    public class Properties : Dictionary<Guid, string>
    {
        public Properties()
        {
        }

        public Properties(IDictionary<Guid, string> dictionary)
            : base(dictionary)
        {
        }
    }
}
