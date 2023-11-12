using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AutoBogus.Templating;
using FluentAssertions;
using Xunit;

namespace AutoBogus.Tests;

public class TemplateFixture
{
    [Fact]
    public void Should_Handle_Strings()
    {
        const string testData =
            " StringField  \r\n" +
            " value1       \r\n" +
            "              \r\n" +
            " $empty$      \r\n";

        var faker = new AutoFaker<Parent>();

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(3);
        result[0].StringField.Should().Be("value1");
        result[1].StringField.Should().BeNull();
        result[2].StringField.Should().BeEmpty();
    }

    [Fact]
    public void Should_Handle_Int()
    {
        const string testData =
            """
            IntField | NullableIntField
            0        |
            1        | 0
            3        | 2
            """;

        var faker = new AutoFaker<Parent>();

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(3);
        result[0].IntField.Should().Be(0);
        result[0].NullableIntField.Should().BeNull();
        result[1].IntField.Should().Be(1);
        result[1].NullableIntField.Should().Be(0);
        result[2].IntField.Should().Be(3);
        result[2].NullableIntField.Should().Be(2);
    }

    [Fact]
    public void Should_Handle_Decimal()
    {
        const string testData =
            """
            DecimalField | NullableDecimalField
            0            |
            1.23         | 0.01
            3.000354     | 2
            """;

        var faker = new AutoFaker<Parent>();

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(3);
        result[0].DecimalField.Should().Be(0);
        result[0].NullableDecimalField.Should().BeNull();
        result[1].DecimalField.Should().Be(1.23m);
        result[1].NullableDecimalField.Should().Be(0.01m);
        result[2].DecimalField.Should().Be(3.000354m);
        result[2].NullableDecimalField.Should().Be(2);
    }

    [Fact]
    public void Should_Handle_Dates()
    {
        const string testData =
            """
            DateTimeField | NullableDateTimeField
            2006-02-28    |
            2010-03-01    | 2011-04-08
            """;

        var faker = new AutoFaker<Parent>();

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(2);
        result[0].DateTimeField.Should().Be(DateTime.Parse("2006-02-28", CultureInfo.InvariantCulture));
        result[0].NullableDateTimeField.Should().BeNull();
        result[1].DateTimeField.Should().Be(DateTime.Parse("2010-03-01",         CultureInfo.InvariantCulture));
        result[1].NullableDateTimeField.Should().Be(DateTime.Parse("2011-04-08", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Should_Generate_Not_Specified_Fields()
    {
        const string testData =
            " StringField  \r\n" +
            " value1       \r\n";

        var faker = new AutoFaker<Parent>();
        faker.RuleFor(p => p.IntField, _ => 999);

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(1);
        result[0].StringField.Should().Be("value1");
        result[0].IntField.Should().Be(999);

        // make sure child got generated
        result.Should().OnlyContain(r => r.Child != null);
    }

    [Fact]
    public void Should_AutoNumber_If_Specified()
    {
        const string testData =
            " StringField  \r\n" +
            " value1       \r\n" +
            " value2       \r\n" +
            " value3       \r\n";

        var faker = new AutoFaker<Parent>();
        faker.Identity(p => p.Id);

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(3);
        result[0].Id.Should().Be(0);
        result[1].Id.Should().Be(1);
        result[2].Id.Should().Be(2);
    }

    [Fact]
    public void Should_Ignore_If_Specified()
    {
        const string testData =
            " IntField  \r\n" +
            " 10       \r\n" +
            " 11       \r\n" +
            " 12       \r\n";

        var faker = new AutoFaker<Parent>();
        faker.Ignore(p => p.StringField);

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(3);
        result.Should().OnlyContain(r => r.StringField == null);
        result[0].IntField.Should().Be(10);
        result[1].IntField.Should().Be(11);
        result[2].IntField.Should().Be(12);
    }

    [Fact]
    public void Should_Use_Type_Converter_If_Specified()
    {
        const string testData =
            " Child  \r\n" +
            " Child1 \r\n" +
            " Child2 \r\n" +
            "        \r\n";

        var binder = new TemplateBinder().SetTypeConverter(ChildConverter());

        var faker = new AutoFaker<Parent>(null, binder);

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(3);
        result[0].Child.Name.Should().Be("child1");
        result[1].Child.Name.Should()
            .NotBe("child1"); // noting that in the converted for child2 we'd use a new faker instance
        result[1].Child.Name.Should().NotBeNull();
        result[2].Child.Should().BeNull();
    }

    [Fact]
    public void Should_Treat_Missing_As_Empty_If_Specified()
    {
        const string testData =
            " StringField  \r\n" +
            "        \r\n";

        var binder = new TemplateBinder()
            .TreatMissingAsEmpty()
            .SetTypeConverter(ChildConverter());

        var faker = new AutoFaker<Parent>(null, binder);

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(1);
        result[0].StringField.Should().BeEmpty();
    }

    [Fact]
    public void Should_Translate_Space_In_Field_If_Specified()
    {
        const string testData =
            """
            Space Field | Date Space Field
            test |
              | 2002-09-08
            """;

        var binder = new TemplateBinder()
            .SetPropertyNameSpaceDelimiter("_");

        var faker = new AutoFaker<Parent>(null, binder);

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(2);
        result[0].Space_Field.Should().Be("test");
        result[0].Date_Space_Field.Should().BeNull();
        result[1].Space_Field.Should().BeNull();
        result[1].Date_Space_Field.Should().Be(DateTime.Parse("2002-09-08", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Should_Handle_Space_After_NewLine()
    {
        const string testData =
            " Id | StringField | \r\n " +
            " 1  | test        | \r\n ";

        var binder = new TemplateBinder()
            .SetPropertyNameSpaceDelimiter("_");

        var faker = new AutoFaker<Parent>(null, binder);

        var result = faker.GenerateWithTemplate(testData);

        result.Should().HaveCount(1);
        result[0].StringField.Should().Be("test");
    }

    private static Func<Type, string, (bool Handled, object? Result)> ChildConverter()
    {
        return (type, value) =>
        {
            if (type == typeof(Child))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return (true, null);
                }

                // use a specific type of generation for this value
                Child instance;
                if (value == "Child1")
                {
                    var faker = new AutoFaker<Child>()
                        .RuleFor(p => p.Name, _ => "child1");
                    instance = faker.Generate();
                }
                else
                {
                    // use std generation
                    instance = AutoFaker.Generate<Child>();
                }

                return (true, instance);
            }

            return (false, null);
        };
    }

    [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed")]
    [SuppressMessage("ReSharper",        "InconsistentNaming")]
    private sealed class Parent
    {
        public int       Id                    { get; }
        public string    StringField           { get; } = null!;
        public int       IntField              { get; }
        public int?      NullableIntField      { get; }
        public decimal   DecimalField          { get; }
        public decimal?  NullableDecimalField  { get; }
        public DateTime  DateTimeField         { get; }
        public DateTime? NullableDateTimeField { get; }
        public Child     Child                 { get; } = null!;
        public TestEnum  TestEnum              { get; set; }
        public string    Space_Field           { get; } = null!;
        public DateTime? Date_Space_Field      { get; } = null!;
    }

    [SuppressMessage("Minor Code Smell", "S2344:Enumeration type names should not have \"Flags\" or \"Enum\" suffixes")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order")]
    private enum TestEnum
    {
        Value0,
        Value1,
    }

    private sealed class Child
    {
        public string Name { get; } = null!;
    }
}
