﻿using AutoBogus.Internal;

namespace AutoBogus.Generators;

internal sealed class DoubleGenerator : IAutoGenerator
{
    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        return context.Faker.Random.Double();
    }
}
