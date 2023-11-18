using AutoBogus.Tests.Models.Simple;
using Xunit;

namespace AutoBogus.Tests;

public class AutoGeneratorOverridesFixture
{
    [Fact]
    public void Should_Initialize_As_Configured()
    {
        AutoFaker.Generate<OverrideClass>(builder =>
        {
            builder
                .WithOverride(new TestOverride(false, context => Assert.Null(context.Instance)))
                .WithOverride(new TestOverride(true,  context => Assert.NotNull(context.Instance)))
                .WithOverride(new TestOverride(false, context => Assert.NotNull(context.Instance)));
        });
    }

    [Fact]
    public void Should_Invoke_Type_Override()
    {
        var value = AutoFaker.Generate<int>();
        var result = AutoFaker.Generate<OverrideClass>(builder =>
        {
            builder.WithOverride(context =>
            {
                var instance = (OverrideClass)context.Instance!;
                var method   = typeof(OverrideId).GetMethod("SetValue")!;

                method.Invoke(instance.Id, new object[] { value, });

                return instance;
            });
        });

        Assert.Equal(value, result.Id.Value);
        Assert.NotNull(result.Name);
        Assert.NotNull(result.Amounts);
        Assert.NotEmpty(result.Amounts);
    }

    private sealed class TestOverride : AutoGeneratorOverride
    {
        public TestOverride(bool preinitialize, Action<AutoGenerateOverrideContext> generator)
        {
            Preinitialize = preinitialize;
            Generator     = generator;
        }

        public override bool Preinitialize { get; }

        private Action<AutoGenerateOverrideContext> Generator { get; }

        public override bool CanOverride(AutoGenerateContext context)
        {
            return context.GenerateType == typeof(OverrideClass);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            Generator.Invoke(context);
        }
    }
}
