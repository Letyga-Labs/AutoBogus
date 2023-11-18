using Xunit;

namespace AutoBogus.Playground;

public class CollectionsFixture
{
    [Fact]
    public void Should_Generate_Collections()
    {
        var c1 = AutoFaker.Generate<ICollection<string>>();
        var c2 = AutoFaker.Generate<IDictionary<string, string>>();
        var c3 = AutoFaker.Generate<IEnumerable<string>>();
        var c4 = AutoFaker.Generate<IList<string>>();
        var c5 = AutoFaker.Generate<IReadOnlyCollection<string>>();
        var c6 = AutoFaker.Generate<IReadOnlyDictionary<string, string>>();
        var c7 = AutoFaker.Generate<IReadOnlyList<string>>();
        var c8 = AutoFaker.Generate<ISet<string>>();

        Assert.NotEmpty(c1);
        Assert.NotEmpty(c2);
        Assert.NotEmpty(c3);
        Assert.NotEmpty(c4);
        Assert.NotEmpty(c5);
        Assert.NotEmpty(c6);
        Assert.NotEmpty(c7);
        Assert.NotEmpty(c8);
    }

    [Fact]
    public void Should_Generate_Collection_Properties()
    {
        var collections = AutoFaker.Generate<Collections>();

        Assert.NotEmpty(collections.C1);
        Assert.NotEmpty(collections.C2);
        Assert.NotEmpty(collections.C3);
        Assert.NotEmpty(collections.C4);
        Assert.NotEmpty(collections.C5);
        Assert.NotEmpty(collections.C6);
        Assert.NotEmpty(collections.C7);
        Assert.NotEmpty(collections.C8);
    }

    [Fact]
    public void Should_Generate_Collection_Properties_With_Rules()
    {
        var faker = new AutoFaker<Collections>()
            .RuleFor(c => c.C1, f => new List<string>
            {
                f.Random.Word(),
            })
            .RuleFor(c => c.C2, f => new Dictionary<string, string>
            {
                { f.Random.Word(), f.Random.Word() },
            })
            .RuleFor(c => c.C3, f => new List<string>
            {
                f.Random.Word(),
            })
            .RuleFor(c => c.C4, f => new List<string>
            {
                f.Random.Word(),
            })
            .RuleFor(c => c.C5, f => new List<string>
            {
                f.Random.Word(),
            })
            .RuleFor(c => c.C6, f => new Dictionary<string, string>
            {
                { f.Random.Word(), f.Random.Word() },
            })
            .RuleFor(c => c.C7, f => new List<string>
            {
                f.Random.Word(),
            })
            .RuleFor(c => c.C8, f => new HashSet<string>
            {
                f.Random.Word(),
            });

        var collections = faker.Generate();

        Assert.NotEmpty(collections.C1);
        Assert.NotEmpty(collections.C2);
        Assert.NotEmpty(collections.C3);
        Assert.NotEmpty(collections.C4);
        Assert.NotEmpty(collections.C5);
        Assert.NotEmpty(collections.C6);
        Assert.NotEmpty(collections.C7);
        Assert.NotEmpty(collections.C8);
    }

    private class Collections
    {
        public ICollection<string>                 C1 { get; set; } = null!;
        public IDictionary<string, string>         C2 { get; set; } = null!;
        public IEnumerable<string>                 C3 { get; set; } = null!;
        public IList<string>                       C4 { get; set; } = null!;
        public IReadOnlyCollection<string>         C5 { get; set; } = null!;
        public IReadOnlyDictionary<string, string> C6 { get; set; } = null!;
        public IReadOnlyList<string>               C7 { get; set; } = null!;
        public ISet<string>                        C8 { get; set; } = null!;
    }
}
