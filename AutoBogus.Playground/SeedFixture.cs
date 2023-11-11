namespace AutoBogus.Playground;

public class SeedFixture
{
    [Fact]
    public void DateTimeOffsetTest()
    {
        int seed = 1;

        var faker1  = new AutoFaker<MyEntity>().UseSeed(seed);
        var faker2  = new AutoFaker<MyEntity>().UseSeed(seed);
        var faker3  = new AutoFaker<MyEntity>();
        var entity1 = faker1.Generate();
        var entity2 = faker2.Generate();
        var entity3 = faker3.Generate();

        entity2.Name.Should().Be(entity1.Name);
        entity2.DeprecationDate.Should().BeCloseTo(entity1.DeprecationDate, 500);

        entity3.Name.Should().NotBe(entity2.Name);
        entity3.DeprecationDate.Should().NotBeCloseTo(entity2.DeprecationDate, 500);
    }

    [Fact]
    public void AuthorTest()
    {
        // Global seed configuration
        Randomizer.Seed = new Random(8675309);

        // Local seed configuration
        const int seed = 1234;
        var authorFaker = new AutoFaker<Author>()
            .Configure(builder => builder
                .WithSkip<Author>(a => a.Id)
                .WithSkip<Author>(a => a.Books))
            .UseSeed(seed);

        // Check output using LINQPad
        var author = authorFaker.Generate();

        author.FirstName.Should().Be("functionalities");
        author.LastName.Should().Be("throughput");
    }

    private sealed class MyEntity
    {
        public DateTimeOffset DeprecationDate { get; set; }
        public string         Name            { get; set; }
    }

    public class Book
    {
        public int    Id   { get; set; }
        public string Name { get; set; }
    }

    public class Author
    {
        public int    Id        { get; set; }
        public string FirstName { get; set; }
        public string LastName  { get; set; }

        // Navigation properties
        public ICollection<Book> Books { get; set; }
    }
}
