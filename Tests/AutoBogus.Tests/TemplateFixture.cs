using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AutoBogus.Templating;
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

        Assert.Equal(3,        result.Count);
        Assert.Equal("value1", result[0].StringField);
        Assert.Null(result[1].StringField);
        Assert.Empty(result[2].StringField);
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

        Assert.Equal(3, result.Count);
        Assert.Equal(0, result[0].IntField);
        Assert.Null(result[0].NullableIntField);
        Assert.Equal(1, result[1].IntField);
        Assert.Equal(0, result[1].NullableIntField);
        Assert.Equal(3, result[2].IntField);
        Assert.Equal(2, result[2].NullableIntField);
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

        Assert.Equal(3, result.Count);
        Assert.Equal(0, result[0].DecimalField);
        Assert.Null(result[0].NullableDecimalField);
        Assert.Equal(1.23m,     result[1].DecimalField);
        Assert.Equal(0.01m,     result[1].NullableDecimalField);
        Assert.Equal(3.000354m, result[2].DecimalField);
        Assert.Equal(2,         result[2].NullableDecimalField);
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

        Assert.Equal(2, result.Count);
        Assert.Null(result[0].NullableDateTimeField);
        Assert.Equal(DateTime.Parse("2006-02-28", CultureInfo.InvariantCulture), result[0].DateTimeField);
        Assert.Equal(DateTime.Parse("2010-03-01", CultureInfo.InvariantCulture), result[1].DateTimeField);
        Assert.Equal(DateTime.Parse("2011-04-08", CultureInfo.InvariantCulture), result[1].NullableDateTimeField);
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

        Assert.Single(result);
        Assert.Equal("value1", result[0].StringField);
        Assert.Equal(999,      result[0].IntField);

        // make sure child got generated
        Assert.All(result, r => Assert.NotNull(r.Child));
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

        Assert.Equal(3, result.Count);
        Assert.Equal(0, result[0].Id);
        Assert.Equal(1, result[1].Id);
        Assert.Equal(2, result[2].Id);
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

        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.NotNull(r.StringField));
        Assert.Equal(10, result[0].IntField);
        Assert.Equal(11, result[1].IntField);
        Assert.Equal(12, result[2].IntField);
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

        Assert.Equal(3,        result.Count);
        Assert.Equal("child1", result[0].Child.Name);
        // noting that in the converted for child2 we'd use a new faker instance
        Assert.NotEqual("child1", result[1].Child.Name);
        Assert.NotNull(result[1].Child.Name);
        Assert.Null(result[2].Child);
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

        Assert.Single(result);
        Assert.Empty(result[0].StringField);
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

        Assert.Equal(2,      result.Count);
        Assert.Equal("test", result[0].Space_Field);
        Assert.Null(result[0].Date_Space_Field);
        Assert.Null(result[1].Space_Field);
        Assert.Equal(DateTime.Parse("2002-09-08", CultureInfo.InvariantCulture), result[1].Date_Space_Field);
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

        Assert.Single(result);
        Assert.Equal("test", result[0].StringField);
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
