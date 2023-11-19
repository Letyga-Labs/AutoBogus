using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using AutoBogus.Internal;
using Bogus;

namespace AutoBogus;

/// <summary>
///     A class used to invoke generation requests of type <typeparamref name="TType" />.
/// </summary>
/// <typeparam name="TType">The type of instance to generate.</typeparam>
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name")]
public class AutoFaker<TType> : Faker<TType>
    where TType : class
{
    private readonly Func<Faker, TType> _defaultCreateAction;

    private AutoFakerConfig? _config;

    private bool _createInitialized;
    private bool _finishInitialized;

    /// <summary>
    ///     Instantiates an instance of the <see cref="AutoFaker{TType}" /> class.
    /// </summary>
    /// <param name="locale">The locale to use for value generation.</param>
    /// <param name="binder">The <see cref="IAutoBinder" /> instance to use for the generation request.</param>
    public AutoFaker(string? locale = null, IAutoBinder? binder = null)
        : base(locale ?? AutoFakerConfig.DefaultLocale, binder)
    {
        Binder = binder;

        // Ensure the default create action is cleared
        // This is so we can check whether it has been set externally
        _defaultCreateAction          = CreateActions[currentRuleSet];
        CreateActions[currentRuleSet] = null;
    }

    /// <summary>
    ///     The <see cref="IAutoBinder" /> instance to use for the generation request.
    /// </summary>
    public IAutoBinder? Binder { get; private set; }

    private AutoFakerConfig Config
    {
        get => _config!;
        set
        {
            Locale = value.Locale;
            Binder = value.Binder;

            // Also pass the binder set up to the underlying Faker
            binder = value.Binder;

            // Apply a configured faker if set
            if (value.FakerHub != null)
            {
                FakerHub = value.FakerHub;
            }

            _config = value;
        }
    }

    /// <summary>
    ///     Configures the current faker instance.
    /// </summary>
    /// <param name="configure">A handler to build the faker configuration.</param>
    /// <returns>The current faker instance.</returns>
    public AutoFaker<TType> Configure(Action<IAutoGenerateConfigBuilder>? configure = null)
    {
        var config  = new AutoFakerConfig(AutoFaker.DefaultConfig);
        var builder = new AutoFakerConfigBuilder(config);
        configure?.Invoke(builder);

        Config = config;

        return this;
    }

    /// <summary>
    ///     Generates an instance of type <typeparamref name="TType" />.
    /// </summary>
    /// <param name="ruleSets">An optional list of delimited rule sets to use for the generate request.</param>
    /// <returns>The generated instance of type <typeparamref name="TType" />.</returns>
    public override TType Generate(string? ruleSets = null)
    {
        var context = CreateContext(ruleSets);

        PrepareCreate(context);
        PrepareFinish(context);

        return base.Generate(ruleSets);
    }

    /// <summary>
    ///     Generates a collection of instances of type <typeparamref name="TType" />.
    /// </summary>
    /// <param name="count">The number of instances to generate.</param>
    /// <param name="ruleSets">An optional list of delimited rule sets to use for the generate request.</param>
    /// <returns>The collection of generated instances of type <typeparamref name="TType" />.</returns>
    public override List<TType> Generate(int count, string? ruleSets = null)
    {
        var context = CreateContext(ruleSets);

        PrepareCreate(context);
        PrepareFinish(context);

        return base.Generate(count, ruleSets);
    }

    /// <summary>
    ///     Populates the provided instance with auto generated values.
    /// </summary>
    /// <param name="instance">The instance to populate.</param>
    /// <param name="ruleSets">An optional list of delimited rule sets to use for the populate request.</param>
    public override void Populate(TType instance, string? ruleSets = null)
    {
        var context = CreateContext(ruleSets);
        PrepareFinish(context);

        base.Populate(instance, ruleSets);
    }

    private AutoGenerateContext CreateContext(string? ruleSets)
    {
        var config = new AutoFakerConfig(_config ?? AutoFaker.DefaultConfig);

        if (!string.IsNullOrWhiteSpace(Locale))
        {
            config.Locale = Locale;
        }

        if (Binder != null)
        {
            config.Binder = Binder;
        }

        return new AutoGenerateContext(FakerHub, config)
        {
            RuleSets = ParseRuleSets(ruleSets),
        };
    }

    private IEnumerable<string> ParseRuleSets(string? ruleSets)
    {
        // Parse and clean the rule set list
        // If the rule set list is empty it defaults to a list containing only 'default'
        // By this point the currentRuleSet should be 'default'
        if (string.IsNullOrWhiteSpace(ruleSets))
        {
            ruleSets = null;
        }

        var results = (ruleSets?.Split(',') ?? new[] { currentRuleSet, })
            .Where(ruleSet => !string.IsNullOrWhiteSpace(ruleSet))
            .Select(ruleSet => ruleSet.Trim());

        return results;
    }

    private void PrepareCreate(AutoGenerateContext context)
    {
        // Check a create handler hasn't previously been set or configured externally
        if (_createInitialized || CreateActions[currentRuleSet] != null)
        {
            return;
        }

        CreateActions[currentRuleSet] = faker => GenerateUnfinishedValueByBogus(context, faker);

        _createInitialized = true;
    }

    private void PrepareFinish(AutoGenerateContext context)
    {
        if (_finishInitialized)
        {
            return;
        }

        // Try and get the registered finish with for the current rule
        FinalizeActions.TryGetValue(currentRuleSet, out var finishWith);

        // Add an internal finish to auto populate any remaining values
        FinishWith((faker, instance) => FinishInstancePopulatingUnfilledMembers(context, faker, instance, finishWith));

        _finishInitialized = true;
    }

    private TType GenerateUnfinishedValueByBogus(AutoGenerateContext context, Faker faker)
    {
        // Only auto create if the 'default' rule set is defined for generation
        // because any specific rule sets are expected to handle the full creation
        if (!context.RuleSets.Contains(currentRuleSet))
        {
            return _defaultCreateAction(faker);
        }

        var type = typeof(TType);

        // Set the current type being generated
        context.ParentType   = null;
        context.GenerateType = type;
        context.GenerateName = null;

        // Get the members that should not be set during construction
        var memberNames = GetRuleSetsMemberNames(context);
        foreach (var memberName in TypeProperties.Keys)
        {
            if (memberNames.Contains(memberName))
            {
                var path = PopulationTargetFiltering.GetSkipPathOfMember(type, memberName);
                context.Config.SkipPaths.Add(path);
            }
        }

        // Let Bogus create an instance according to registered rules.
        // Remaining unfilled members will be populated in the FinalizeAction registered
        // by PrepareFinish (context.Binder.PopulateInstance<TType>).
        var unfinishedInstance = context.Binder.CreateUnpopulatedInstance<TType>(context);

        return unfinishedInstance;
    }

    private void FinishInstancePopulatingUnfilledMembers(
        AutoGenerateContext    context,
        Faker                  faker,
        TType                  instance,
        FinalizeAction<TType>? finishWith)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var type = instance.GetType();

        // If dynamic objects are supported, populate as a dictionary
        if (type == typeof(ExpandoObject))
        {
            // Get the expando generator and populate the instance
            _ = Generation.Generate(
                context,
                parentType: null,
                generateType: type,
                generateName: null,
                instance: instance);

            // Clear the context instance
            context.Instance = null;
        }
        else
        {
            // Finalize the instance population by AutoBinder
            var memberNames = GetRuleSetsMemberNames(context);
            var members = TypeProperties
                .Where(it => !memberNames.Contains(it.Key))
                .Select(it => it.Value)
                .ToList();

            context.Binder.PopulateInstance<TType>(instance, context, members);
        }

        // Ensure the standard Bogus finish action is invoked
        finishWith?.Action(faker, instance);
    }

    /// <summary>
    ///     Get the member names from all the rule sets being used to generate the instance.
    /// </summary>
    private List<string> GetRuleSetsMemberNames(AutoGenerateContext context)
    {
        var members = new List<string>();
        foreach (var ruleSetName in context.RuleSets)
        {
            if (Actions.TryGetValue(ruleSetName, out var ruleSet))
            {
                members.AddRange(ruleSet.Keys);
            }
        }

        return members;
    }
}
