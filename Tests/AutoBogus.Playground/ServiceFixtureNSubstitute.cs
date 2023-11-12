using AutoBogus.NSubstitute;
using Xunit.Abstractions;

namespace AutoBogus.Playground;

public class ServiceFixtureNSubstitute
    : ServiceFixture
{
    public ServiceFixtureNSubstitute(ITestOutputHelper output)
        : base(output, new NSubstituteBinder())
    {
    }
}
