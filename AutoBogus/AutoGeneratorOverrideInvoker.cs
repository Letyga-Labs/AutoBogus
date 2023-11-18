using System.Diagnostics;

namespace AutoBogus;

internal sealed class AutoGeneratorOverrideInvoker : IAutoGenerator
{
    private readonly IAutoGenerator                       _generator;
    private readonly IReadOnlyList<AutoGeneratorOverride> _overrides;

    internal AutoGeneratorOverrideInvoker(IAutoGenerator generator, IReadOnlyList<AutoGeneratorOverride> overrides)
    {
        _generator = generator;
        _overrides = overrides;
    }

    /// <summary>
    /// Visible for tests.
    /// </summary>
    internal IReadOnlyList<AutoGeneratorOverride> Overrides => _overrides;

    object IAutoGenerator.Generate(AutoGenerateContext context)
    {
        var overrideContext = new AutoGenerateOverrideContext(context);

        foreach (var generatorOverride in _overrides)
        {
            // Check if an initialized instance is needed
            if (generatorOverride.Preinitialize && overrideContext.Instance == null)
            {
                overrideContext.Instance = _generator.Generate(context);
            }

            // Let each override apply updates to the instance
            generatorOverride.Generate(overrideContext);
        }

        Debug.Assert(overrideContext.Instance != null, "overrideContext.Instance != null");
        return overrideContext.Instance;
    }
}
