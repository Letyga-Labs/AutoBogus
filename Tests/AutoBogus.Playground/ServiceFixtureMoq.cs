using AutoBogus.Moq;
using Xunit.Abstractions;

namespace AutoBogus.Playground;

public class ServiceFixtureMoq : ServiceFixture
{
    public ServiceFixtureMoq(ITestOutputHelper output)
        : base(output, new MoqBinder())
    {
    }
}
