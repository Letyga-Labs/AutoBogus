﻿using AutoBogus.Internal;

namespace AutoBogus.Generators;

internal sealed class DateTimeGenerator : IAutoGenerator
{
    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        return context.Faker.Date.Recent();
    }
}
