using AutoBogus.Internal;

namespace AutoBogus.Generators;

internal sealed class CustomGeneratorProvidedByClientOverrides : IAutoGenerator
{
    private readonly IAutoGenerator                       _defaultGenerator;
    private readonly IReadOnlyList<AutoGeneratorOverride> _clientOverrides;

    internal CustomGeneratorProvidedByClientOverrides(
        IAutoGenerator                       defaultGenerator,
        IReadOnlyList<AutoGeneratorOverride> clientOverrides)
    {
        _defaultGenerator = defaultGenerator;
        _clientOverrides  = clientOverrides;
    }

    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        var overrideContext = new AutoGenerateOverrideContext(context);

        foreach (var generatorOverride in _clientOverrides)
        {
            // Check if an initialized instance is needed
            if (generatorOverride.Preinitialize && overrideContext.Instance == null)
            {
                overrideContext.Instance = _defaultGenerator.Generate(context);
            }

            // Let each override apply updates to the instance
            overrideContext.Override = generatorOverride;
            generatorOverride.Generate(overrideContext);
        }

        return overrideContext.Instance!;
    }
}
