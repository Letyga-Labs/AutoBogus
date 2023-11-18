using Bogus;

namespace AutoBogus;

internal sealed class AutoConfig
{
    internal const string DefaultLocale             = "en";
    internal const int    GenerateAttemptsThreshold = 3;

    internal static readonly Func<AutoGenerateContext, int>  DefaultRepeatCount       = _ => 3;
    internal static readonly Func<AutoGenerateContext, int>  DefaultDataTableRowCount = _ => 15;
    internal static readonly Func<AutoGenerateContext, int>  DefaultRecursiveDepth    = _ => 2;
    internal static readonly Func<AutoGenerateContext, int?> DefaultTreeDepth         = _ => null;

    internal AutoConfig()
    {
        Locale            = DefaultLocale;
        RepeatCount       = DefaultRepeatCount;
        DataTableRowCount = DefaultDataTableRowCount;
        RecursiveDepth    = DefaultRecursiveDepth;
        TreeDepth         = DefaultTreeDepth;
        Binder            = new AutoBinder();
        SkipTypes         = new HashSet<Type>();
        SkipPaths         = new HashSet<string>();
        Overrides         = new HashSet<AutoGeneratorOverride>();
    }

    internal AutoConfig(AutoConfig config)
    {
        Locale            = config.Locale;
        RepeatCount       = config.RepeatCount;
        DataTableRowCount = config.DataTableRowCount;
        RecursiveDepth    = config.RecursiveDepth;
        TreeDepth         = config.TreeDepth;
        Binder            = config.Binder;
        FakerHub          = config.FakerHub;
        SkipTypes         = config.SkipTypes.ToHashSet();
        SkipPaths         = config.SkipPaths.ToHashSet();
        Overrides         = config.Overrides.ToHashSet();
    }

    internal string          Locale    { get; set; }
    internal IAutoBinder     Binder    { get; set; }
    internal Faker?          FakerHub  { get; set; }
    internal HashSet<Type>   SkipTypes { get; set; }
    internal HashSet<string> SkipPaths { get; set; }

    internal Func<AutoGenerateContext, int>  RepeatCount       { get; set; }
    internal Func<AutoGenerateContext, int>? DataTableRowCount { get; set; }
    internal Func<AutoGenerateContext, int>  RecursiveDepth    { get; set; }
    internal Func<AutoGenerateContext, int?> TreeDepth         { get; set; }

    internal HashSet<AutoGeneratorOverride> Overrides { get; set; }
}
