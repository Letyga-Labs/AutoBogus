using System.Diagnostics.CodeAnalysis;
using Bogus;
using Xunit;

namespace AutoBogus.Playground;

public class SeedFixture
{
    [Fact]
    public void DateTimeOffsetTest()
    {
        const int seed = 1;

        var faker1  = new AutoFaker<MyEntity>().UseSeed(seed);
        var faker2  = new AutoFaker<MyEntity>().UseSeed(seed);
        var faker3  = new AutoFaker<MyEntity>();
        var entity1 = faker1.Generate();
        var entity2 = faker2.Generate();
        var entity3 = faker3.Generate();

        Assert.Equal(entity1.Name, entity2.Name);
        Assert.NotEqual(entity2.Name, entity3.Name);

        Assert.Equal(entity1.DeprecationDate, entity2.DeprecationDate, TimeSpan.FromMilliseconds(500));
        Assert.NotEqual(
            entity2.DeprecationDate,
            entity3.DeprecationDate,
            (a, b) => (a - b).Duration() < TimeSpan.FromMilliseconds(500));
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

        Assert.Equal(0, author.Id);
        Assert.Null(author.Books);
        Assert.Equal("functionalities", author.FirstName);
        Assert.Equal("throughput",      author.LastName);
    }

    public class Book
    {
        public int    Id   { get; set; }
        public string Name { get; set; } = null!;
    }

    [SuppressMessage("Usage", "CA2227:Collection properties should be read only")]
    public class Author
    {
        public int    Id        { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName  { get; set; } = null!;

        // Navigation properties
        public ICollection<Book> Books { get; set; } = null!;
    }

    private sealed class MyEntity
    {
        [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed")]
        public DateTimeOffset DeprecationDate { get; set; }

        public string Name { get; set; } = null!;
    }
}
