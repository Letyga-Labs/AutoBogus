using AutoBogus.Internal;

namespace AutoBogus.Generators;

internal sealed class IpAddressGenerator : IAutoGenerator
{
    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        return context.Faker.Internet.IpAddress();
    }
}
