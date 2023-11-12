using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Xunit;
using Xunit.Abstractions;

namespace AutoBogus.Playground;

public class StructFixture
{
    private readonly ITestOutputHelper _outputHelper;

    public StructFixture(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void Generate_ExampleStruct()
    {
        var faker = AutoFaker.Create(builder =>
        {
            builder.WithOverride(new ExampleStructOverride());
        });

        var exampleStruct = faker.Generate<ExampleStruct>();

        _outputHelper.WriteLine(exampleStruct.Month.ToString(CultureInfo.InvariantCulture));

        Assert.True(exampleStruct.Month > 0 && exampleStruct.Month <= 12);
    }
}

[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order")]
public struct ExampleStruct : IEquatable<ExampleStruct>
{
    public ExampleStruct(int month)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(
                nameof(month),
                $"Value should be in range [1-12]\nActual value was {month}."
            );
        }

        Month = month;
    }

    public int Month { get; }

    public bool Equals(ExampleStruct other)
    {
        return Month == other.Month;
    }

    public override bool Equals(object? obj)
    {
        return obj is ExampleStruct other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Month;
    }

    public static bool operator ==(ExampleStruct left, ExampleStruct right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ExampleStruct left, ExampleStruct right)
    {
        return !left.Equals(right);
    }
}

[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type")]
internal class ExampleStructOverride : AutoGeneratorOverride
{
    public override bool Preinitialize => false;

    public override bool CanOverride(AutoGenerateContext context)
    {
        return context.GenerateType == typeof(ExampleStruct);
    }

    public override void Generate(AutoGenerateOverrideContext context)
    {
        context.Instance = new ExampleStruct(5);
    }
}
