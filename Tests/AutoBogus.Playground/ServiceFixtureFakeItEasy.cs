using AutoBogus.FakeItEasy;
using Xunit.Abstractions;

namespace AutoBogus.Playground;

public class ServiceFixtureFakeItEasy : ServiceFixture
{
    public ServiceFixtureFakeItEasy(ITestOutputHelper output)
        : base(output, new FakeItEasyBinder())
    {
    }
}
