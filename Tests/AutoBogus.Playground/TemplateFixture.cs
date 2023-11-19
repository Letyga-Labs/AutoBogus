using AutoBogus.Templating;
using Xunit;

namespace AutoBogus.Playground;

public class TemplateFixture
{
    [Fact]
    public void TestAutoFaker()
    {
        var binder = new TemplateBinder();
        var persons = new AutoFaker<Person>(null, binder)
            .GenerateWithTemplate(
                """
                Id | FirstName | LastName
                0  | John      | Smith
                1  | Jane      | Jones
                2  | Bob       | Clark
                """);

        Assert.False(string.IsNullOrWhiteSpace(persons[0].Status));
        Assert.False(string.IsNullOrWhiteSpace(persons[1].Status));
        Assert.False(string.IsNullOrWhiteSpace(persons[2].Status));

        var expected = new List<Person>
        {
            new()
            {
                Id        = 0,
                FirstName = "John",
                LastName  = "Smith",
                Status    = persons[0].Status,
            },
            new()
            {
                Id        = 1,
                FirstName = "Jane",
                LastName  = "Jones",
                Status    = persons[1].Status,
            },
            new()
            {
                Id        = 2,
                FirstName = "Bob",
                LastName  = "Clark",
                Status    = persons[2].Status,
            },
        };

        Assert.Equivalent(expected, persons);
    }

    private class Person
    {
        public int    Id        { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName  { get; set; } = null!;
        public string Status    { get; set; } = null!;
    }
}
