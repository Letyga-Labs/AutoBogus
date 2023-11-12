using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Bogus;

namespace AutoBogus.Templating;

public static class TemplateExtensions
{
    /// <summary>
    ///     Generate data using data values in template and AutoFaker for other values.
    /// </summary>
    /// <typeparam name="TType">Type of the genetaed data.</typeparam>
    /// <returns>Generated data.</returns>
    public static List<TType> GenerateWithTemplate<TType>(this AutoFaker<TType> src, string template)
        where TType : class
    {
        var templator = new Templator<TType>(src);
        var result    = templator.GenerateFromTemplate(template);
        return result;
    }

    /// <summary>
    ///     Generate data using the supplied rows but only those proprties who appear in headers list
    ///     Properties not in the headers list will use AutoFaker rules.
    /// </summary>
    /// <typeparam name="TType">Type of the genetaed data.</typeparam>
    /// <returns>Generated data.</returns>
    public static List<TType> GenerateWithTemplate<TType>(
        this AutoFaker<TType> src,
        List<string>          headers,
        List<TType>           rows)
        where TType : class
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);
        var templator = new Templator<TType>(src);
        var result    = templator.GenerateFromTemplate(headers, rows);
        return result;
    }

    /// <summary>
    ///     Will autonumber the identity property.
    /// </summary>
    /// <typeparam name="TType">TYpe of the generated data.</typeparam>
    /// <typeparam name="TProperty">Type of property which is to be autonumbered.</typeparam>
    /// <param name="src">The AutoBinder instance being configured.</param>
    /// <param name="property">Expression denoting property which is to be autonumbered.</param>
    /// <returns>The given AutoBinder instance.</returns>
    public static AutoFaker<TType> Identity<TType, TProperty>(
        this AutoFaker<TType>              src,
        Expression<Func<TType, TProperty>> property)
        where TType : class
    {
        var rule = new FakerRule<TType, TProperty>
        {
            Property = property,
            Setter = f =>
            {
                var genericType  = typeof(TProperty);
                var castMethod   = typeof(TemplateExtensions).GetMethod("Cast")!.MakeGenericMethod(genericType);
                var castedObject = castMethod.Invoke(null, new object[] { f.IndexFaker, });
                return (TProperty)castedObject!;
            },
        };

        src.RegisterRule(rule);

        return src;
    }

    /// <summary>
    ///     Tells the generator not to generate a value, but to use the default value for the type.
    /// </summary>
    /// <typeparam name="TType">TYpe of the generated data.</typeparam>
    /// <typeparam name="TProperty">Type of property for which the default values is to be used.</typeparam>
    /// <param name="src">The AutoBinder instance being configured.</param>
    /// <param name="property">Expression denoting property for which the default value is to be used.</param>
    /// <returns>The given AutoBinder instance.</returns>
    public static AutoFaker<TType> Ignore<TType, TProperty>(
        this AutoFaker<TType>              src,
        Expression<Func<TType, TProperty>> property)
        where TType : class
    {
        ArgumentNullException.ThrowIfNull(property);

        // if we ignore we will get the default value for the type
        var defaultObj = Activator.CreateInstance<TType>();

        var prop = typeof(TType).GetProperty(
            ReflectionHelper.GetMemberName(property.Body),
            BindingFlags.Public | BindingFlags.Instance);

        var valueOfProperty = (TProperty)prop!.GetValue(defaultObj)!;

        var setter = new Func<Faker, TProperty>(_ => valueOfProperty);

        var rule = new FakerRule<TType, TProperty>
        {
            Property = property,
            Setter   = setter,
        };

        src.RegisterRule(rule);

        return src;
    }

    public static void RegisterRule<TType>(this AutoFaker<TType> faker, FakerRule<TType> fakerRule)
        where TType : class
    {
        ArgumentNullException.ThrowIfNull(faker);
        ArgumentNullException.ThrowIfNull(fakerRule);

        // dynamically cast object to correct type
        var genericRuleType  = typeof(FakerRule<,>);
        var specificRuleType = genericRuleType.MakeGenericType(typeof(TType), fakerRule.PropertyType);
        var castMethod       = typeof(TemplateExtensions).GetMethod("Cast")!.MakeGenericMethod(specificRuleType);
        var castedObject     = castMethod.Invoke(null, new object[] { fakerRule, });

        // dynamically call the RuleFor method on faker object
        var ruleForMethod = ReflectionHelper.GetMethod(
            faker.GetType(),
            "RuleFor",
            new[] { fakerRule.PropertyType, },
            new[]
            {
                ReflectionHelper.GetPropType(castedObject!, "Property"),
                ReflectionHelper.GetPropType(castedObject!, "Setter"),
            });

        ruleForMethod.Invoke(
            faker,
            new[]
            {
                ReflectionHelper.GetPropValue(castedObject!, "Property"),
                ReflectionHelper.GetPropValue(castedObject!, "Setter"),
            });
    }

    public static TCast Cast<TCast>(object o)
    {
        return (TCast)o;
    }

    [SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed")]
    public abstract class FakerRule<T>
    {
        public abstract Type PropertyType { get; }
    }

    public class FakerRule<T, TProperty> : FakerRule<T>
    {
        public Expression<Func<T, TProperty>> Property { get; set; } = null!;

        public Func<Faker, TProperty> Setter { get; set; } = null!;

        public override Type PropertyType => typeof(TProperty);
    }
}
