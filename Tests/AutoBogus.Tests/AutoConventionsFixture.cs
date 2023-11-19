using AutoBogus.Conventions;
using Xunit;

namespace AutoBogus.Tests;

public class AutoConventionsFixture
{
    private IAutoFaker _faker = null!;

    [Fact]
    public void Should_Apply_Conventions()
    {
        _faker = AutoFaker.Create(builder =>
        {
            builder.WithConventions();
        });

        var instance = _faker.Generate<TestClass>();
        Assert.Contains("@", instance.Email, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_Apply_Conventions_For_Alias()
    {
        _faker = AutoFaker.Create(builder =>
        {
            builder.WithConventions(c => c.Email.Aliases("anotheremail", "  "));
        });

        var instance = _faker.Generate<TestClass>();
        Assert.Contains("@", instance.AnotherEmail, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_Not_Apply_Conventions_For_Disabled_Generator()
    {
        _faker = AutoFaker.Create(builder =>
        {
            builder.WithConventions(c => c.Email.Enabled = false);
        });

        var instance = _faker.Generate<TestClass>();
        Assert.DoesNotContain("@", instance.Email, StringComparison.Ordinal);
    }

    [Fact]
    public void Should_Not_Apply_Conventions_When_AlwaysGenerate_Is_False()
    {
        _faker = AutoFaker.Create(builder =>
        {
            builder
                .WithOverride(new TestGeneratorOverride())
                .WithConventions(c => c.AlwaysGenerate = false);
        });

        var instance = _faker.Generate<TestClass>();
        Assert.Equal("EMAIL", instance.Email);
    }

    [Fact]
    public void Should_Not_Apply_Conventions_When_Generator_AlwaysGenerate_Is_False()
    {
        _faker = AutoFaker.Create(builder =>
        {
            builder
                .WithOverride(new TestGeneratorOverride())
                .WithConventions(c => c.Email.AlwaysGenerate = false);
        });

        var instance = _faker.Generate<TestClass>();
        Assert.Equal("EMAIL", instance.Email);
    }

    private class TestClass
    {
        public string Email        { get; set; } = null!;
        public string AnotherEmail { get; set; } = null!;
    }

    private class TestGeneratorOverride
        : AutoGeneratorOverride
    {
        public override bool CanOverride(AutoGenerateContext context)
        {
            return context.GenerateName == "Email";
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            context.Instance = "EMAIL";
        }
    }
}
