﻿using AutoBogus.Internal;

namespace AutoBogus.Generators;

internal sealed class ByteGenerator : IAutoGenerator
{
    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        return context.Faker.Random.Byte();
    }
}
