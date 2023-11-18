using System.Diagnostics;
using System.Dynamic;
using System.Net;
using AutoBogus.Generation;
using AutoBogus.Generators;
using AutoBogus.Util;

namespace AutoBogus;

internal static class AutoGeneratorFactory
{
    internal static readonly IDictionary<Type, IAutoGenerator> Generators = new Dictionary<Type, IAutoGenerator>
    {
        [typeof(bool)]           = new BoolGenerator(),
        [typeof(byte)]           = new ByteGenerator(),
        [typeof(char)]           = new CharGenerator(),
        [typeof(DateTime)]       = new DateTimeGenerator(),
        [typeof(DateTimeOffset)] = new DateTimeOffsetGenerator(),
        [typeof(decimal)]        = new DecimalGenerator(),
        [typeof(double)]         = new DoubleGenerator(),
        [typeof(float)]          = new FloatGenerator(),
        [typeof(Guid)]           = new GuidGenerator(),
        [typeof(int)]            = new IntGenerator(),
        [typeof(IPAddress)]      = new IpAddressGenerator(),
        [typeof(long)]           = new LongGenerator(),
        [typeof(sbyte)]          = new SByteGenerator(),
        [typeof(short)]          = new ShortGenerator(),
        [typeof(string)]         = new StringGenerator(),
        [typeof(uint)]           = new UIntGenerator(),
        [typeof(ulong)]          = new ULongGenerator(),
        [typeof(Uri)]            = new UriGenerator(),
        [typeof(ushort)]         = new UShortGenerator(),
    };

    /// <summary>
    /// Should be obtained only in <see cref="Generator"/>.
    /// Internal visiblity is only for tests.
    /// </summary>
    /// <returns>Resolved generator for the given generation context.</returns>
    internal static IAutoGenerator GetGenerator(AutoGenerateContext context)
    {
        var generator = ResolveGenerator(context);

        // Check if any overrides are available for this generate request
        var overrides = context.Overrides.Where(it => it.CanOverride(context)).ToList();

        if (overrides.Any())
        {
            return new AutoGeneratorOverrideInvoker(generator, overrides);
        }

        return generator;
    }

    /// <summary>
    /// Internal visiblity is only for tests.
    /// </summary>
    /// <returns>Resolved generator for the given generation context.</returns>
    internal static IAutoGenerator ResolveGenerator(AutoGenerateContext context)
    {
        var type = context.GenerateType;

        Debug.Assert(type != null, nameof(type) + " != null");

        // Need check if the type is an in/out parameter and adjusted accordingly
        if (type.IsByRef)
        {
            type = type.GetElementType();
            Debug.Assert(type != null, nameof(type) + " != null");
        }

        // Check if an expando object needs to generator
        // This actually means an existing dictionary needs to be populated
        if (type == typeof(ExpandoObject))
        {
            return new ExpandoObjectGenerator();
        }

        // Do some type -> generator mapping
        if (type.IsArray)
        {
            type = type.GetElementType();
            Debug.Assert(type != null, nameof(type) + " != null");
            return CreateGenericGenerator(typeof(ArrayGenerator<>), type);
        }

        if (DataTableGenerator.TryCreateGenerator(type, out var dataTableGenerator))
        {
            return dataTableGenerator;
        }

        if (DataSetGenerator.TryCreateGenerator(type, out var dataSetGenerator))
        {
            return dataSetGenerator;
        }

        if (type.IsEnum)
        {
            return CreateGenericGenerator(typeof(EnumGenerator<>), type);
        }

        if (ReflectionHelper.IsValueNullable(type))
        {
            type = type.GetGenericArguments().Single();
            return CreateGenericGenerator(typeof(NullableGenerator<>), type);
        }

        var genericCollectionType = ReflectionHelper.GetGenericCollectionType(type);

        if (genericCollectionType != null)
        {
            // For generic types we need to interrogate the inner types
            var generics = genericCollectionType.GetGenericArguments();

            if (ReflectionHelper.IsReadOnlyDictionary(genericCollectionType))
            {
                var keyType   = generics[0];
                var valueType = generics[1];

                return CreateGenericGenerator(typeof(ReadOnlyDictionaryGenerator<,>), keyType, valueType);
            }

            if (ReflectionHelper.IsDictionary(genericCollectionType))
            {
                return CreateDictionaryGenerator(generics);
            }

            if (ReflectionHelper.IsList(genericCollectionType))
            {
                var elementType = generics.Single();
                return CreateGenericGenerator(typeof(ListGenerator<>), elementType);
            }

            if (ReflectionHelper.IsSet(genericCollectionType))
            {
                var elementType = generics.Single();
                return CreateGenericGenerator(typeof(SetGenerator<>), elementType);
            }

            if (ReflectionHelper.IsCollection(genericCollectionType))
            {
                var elementType = generics.Single();
                return CreateGenericGenerator(typeof(ListGenerator<>), elementType);
            }

            // Not a full list type, we can't fake it if it's anything other than
            // the actual IEnumerable<T> interface itelf.
            if (ReflectionHelper.IsEnumerable(genericCollectionType) && type == genericCollectionType)
            {
                var elementType = generics.Single();
                return CreateGenericGenerator(typeof(EnumerableGenerator<>), elementType);
            }
        }

        // Resolve the generator from the type
        if (Generators.TryGetValue(type, out var generator))
        {
            return generator;
        }

        return CreateGenericGenerator(typeof(TypeGenerator<>), type);
    }

    private static IAutoGenerator CreateDictionaryGenerator(Type[] generics)
    {
        var keyType   = generics[0];
        var valueType = generics[1];

        return CreateGenericGenerator(typeof(DictionaryGenerator<,>), keyType, valueType);
    }

    private static IAutoGenerator CreateGenericGenerator(Type generatorType, params Type[] genericTypes)
    {
        var type = generatorType.MakeGenericType(genericTypes);
        return (IAutoGenerator)Activator.CreateInstance(type)!;
    }
}
