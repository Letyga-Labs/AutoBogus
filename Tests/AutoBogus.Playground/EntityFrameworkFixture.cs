using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace AutoBogus.Playground;

[SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
public class EntityFrameworkFixture
{
    [Fact]
    public void TestAutoFaker()
    {
        var parent = AutoFaker.Generate<Parent>(builder => builder.WithTreeDepth(2));
        Assert.NotNull(parent);
    }

    public class Parent
    {
        public virtual Other              Other    { get; set; } = null!;
        public virtual ICollection<Child> Children { get; set; } = null!;
    }

    public class Child
    {
        public virtual Parent             Parent { get; set; } = null!;
        public virtual ICollection<Other> Items  { get; set; } = null!;
    }

    public class Other
    {
        public virtual Child             Child { get; set; } = null!;
        public virtual ICollection<Item> Items { get; set; } = null!;
    }

    public class Item
    {
        public virtual Other Other { get; set; } = null!;
    }
}
