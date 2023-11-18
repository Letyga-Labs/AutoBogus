using AutoBogus.Moq;
using AutoBogus.Tests.Models;
using AutoBogus.Tests.Models.Complex;
using AutoBogus.Tests.Models.Simple;
using FluentAssertions;
using Xunit;

namespace AutoBogus.Tests;

public class MoqBinderFixture
{
    private readonly IAutoFaker _faker;

    public MoqBinderFixture()
    {
        _faker = AutoFaker.Create(builder =>
        {
            builder.WithBinder<MoqBinder>();
        });
    }

    [Fact]
    public void Should_Create_With_Mocks()
    {
        _faker.Generate<Order>().Should().BeGeneratedWithMocks();
    }

    [Fact]
    public void Should_Create_Interface_Mock()
    {
        Assert.NotNull(_faker.Generate<ITestInterface>());
    }

    [Fact]
    public void Should_Create_Abstract_Mock()
    {
        Assert.NotNull(_faker.Generate<TestAbstractClass>());
    }
}
