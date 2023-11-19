using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using AutoBogus.Generators;
using AutoBogus.Internal;
using AutoBogus.Tests.Models.Simple;
using Bogus;
using Xunit;
using DataSet = System.Data.DataSet;

namespace AutoBogus.Tests;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented")]
public partial class AutoGeneratorsFixture
{
    private static Type GetGeneratorType(Type type, params Type[] types)
    {
        return type.MakeGenericType(types);
    }

    private static IAutoGenerator CreateGenerator(Type type, params Type[] types)
    {
        type = GetGeneratorType(type, types);
        return (IAutoGenerator)Activator.CreateInstance(type)!;
    }

    private object InvokeGenerator(Type type, IAutoGenerator generator, object? instance = null)
    {
        var context = CreateContext(type);
        context.Instance = instance;

        return generator.Generate(context);
    }

    private AutoGenerateContext CreateContext(
        Type                            type,
        HashSet<AutoGeneratorOverride>? generatorOverrides       = null,
        Func<AutoGenerateContext, int>? dataTableRowCountFunctor = null)
    {
        var faker  = new Faker();
        var config = new AutoFakerConfig();

        if (generatorOverrides != null)
        {
            config.Overrides = generatorOverrides;
        }

        if (dataTableRowCountFunctor != null)
        {
            config.DataTableRowCount = dataTableRowCountFunctor;
        }

        return new AutoGenerateContext(faker, config)
        {
            GenerateType = type,
        };
    }

    [SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix")]
    public static class Factory
    {
        public class ReadOnlyDictionary
        {
            public static IEnumerable<object[]> ListOfReadOnlyDictionaryTypes
            {
                get
                {
                    yield return new[] { typeof(NonGeneric), };
                    yield return new[] { typeof(OneArgument<int>), };
                    yield return new[]
                    {
                        typeof(TwoArgumentsThatAreDifferentFromBaseReadOnlyDictionaryClass<string, int>),
                    };
                }
            }

            [Theory]
            [MemberData(nameof(ListOfReadOnlyDictionaryTypes))]
            public void Should_Handle_Subclasses(Type readOnlyDictionaryType)
            {
                // Arrange
                var config = new AutoFakerConfig();

                var context = new AutoGenerateContext(config);

                context.GenerateType = readOnlyDictionaryType;

                // Act
                var generator = GeneratorFactory.ResolveGenerator(context);

                var instance = generator.Generate(context);

                // Arrange
                Assert.IsType<ReadOnlyDictionaryGenerator<int, string>>(generator);

                Assert.IsType(readOnlyDictionaryType, instance);
            }

            public class BaseReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
                where TKey : notnull
            {
                private readonly Dictionary<TKey, TValue> _store = new();

                public BaseReadOnlyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items)
                {
                    ArgumentNullException.ThrowIfNull(items);
                    foreach (var item in items)
                    {
                        _store[item.Key] = item.Value;
                    }
                }

                public IEnumerable<TKey>   Keys   => _store.Keys;
                public IEnumerable<TValue> Values => _store.Values;

                public int Count => _store.Count;

                public TValue this[TKey key] => _store[key];

                public bool ContainsKey(TKey key)
                {
                    return _store.ContainsKey(key);
                }

                public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
                {
                    return _store.GetEnumerator();
                }

                public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
                {
                    return _store.TryGetValue(key, out value);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return _store.GetEnumerator();
                }
            }

            public class NonGeneric : BaseReadOnlyDictionary<int, string>
            {
                public NonGeneric(IEnumerable<KeyValuePair<int, string>> items)
                    : base(items)
                {
                }
            }

            public class OneArgument<T> : BaseReadOnlyDictionary<T, string>
                where T : notnull
            {
                public OneArgument(IEnumerable<KeyValuePair<T, string>> items)
                    : base(items)
                {
                }
            }

            public class TwoArgumentsThatAreDifferentFromBaseReadOnlyDictionaryClass<TValue, TKey> :
                BaseReadOnlyDictionary<TKey, TValue>
                where TKey : notnull
            {
                public TwoArgumentsThatAreDifferentFromBaseReadOnlyDictionaryClass(
                    IEnumerable<KeyValuePair<TKey, TValue>> items)
                    : base(items)
                {
                }
            }
        }

        public class Dictionary
        {
            public static IEnumerable<object[]> ListOfDictionaryTypes
            {
                get
                {
                    yield return new[] { typeof(NonGeneric), };
                    yield return new[] { typeof(OneArgument<int>), };
                    yield return new[] { typeof(TwoArgumentsThatAreDifferentFromBaseDictionaryClass<string, int>), };
                }
            }

            [Theory]
            [MemberData(nameof(ListOfDictionaryTypes))]
            public void Should_Handle_Subclasses(Type dictionaryType)
            {
                // Arrange
                var config = new AutoFakerConfig();

                var context = new AutoGenerateContext(config);

                context.GenerateType = dictionaryType;

                // Act
                var generator = GeneratorFactory.ResolveGenerator(context);

                var instance = generator.Generate(context);

                // Arrange
                Assert.IsType<DictionaryGenerator<int, string>>(generator);

                Assert.IsType(dictionaryType, instance);
            }

            public class NonGeneric : Dictionary<int, string>
            {
            }

            public class OneArgument<T> : Dictionary<T, string>
                where T : notnull
            {
            }

            public class TwoArgumentsThatAreDifferentFromBaseDictionaryClass<TValue, TKey> :
                Dictionary<TKey, TValue>
                where TKey : notnull
            {
            }
        }

        public class SetTests
        {
            public static IEnumerable<object[]> ListOfSetTypes
            {
                get
                {
                    yield return new[] { typeof(NonGeneric), };
                    yield return new[] { typeof(GenericWithDifferentType<string>), };
                }
            }

            [Theory]
            [MemberData(nameof(ListOfSetTypes))]
            public void Should_Handle_Subclasses(Type setType)
            {
                // Arrange
                var config = new AutoFakerConfig();

                var context = new AutoGenerateContext(config);

                context.GenerateType = setType;

                // Act
                var generator = GeneratorFactory.ResolveGenerator(context);

                var instance = generator.Generate(context);

                // Arrange
                Assert.IsType<SetGenerator<int>>(generator);

                Assert.IsType(setType, instance);
            }

            public class NonGeneric : HashSet<int>
            {
            }

            public class GenericWithDifferentType<TType> : HashSet<int>
            {
                public TType Property { get; set; } = default!;
            }
        }

        public class List
        {
            public static IEnumerable<object[]> ListOfListTypes
            {
                get
                {
                    yield return new[] { typeof(NonGeneric), };
                    yield return new[] { typeof(GenericWithDifferentType<string>), };
                }
            }

            [Theory]
            [MemberData(nameof(ListOfListTypes))]
            public void Should_Handle_Subclasses(Type listType)
            {
                // Arrange
                var config = new AutoFakerConfig();

                var context = new AutoGenerateContext(config);

                context.GenerateType = listType;

                // Act
                var generator = GeneratorFactory.ResolveGenerator(context);

                var instance = generator.Generate(context);

                // Arrange
                Assert.IsType<ListGenerator<int>>(generator);

                Assert.IsType(listType, instance);
            }

            public class NonGeneric : List<int>
            {
            }

            public class GenericWithDifferentType<TType> : List<int>
            {
                public TType Property { get; set; } = default!;
            }
        }
    }

    public class ReferenceTypes
        : AutoGeneratorsFixture
    {
        [Fact]
        public void Should_Not_Throw_For_Reference_Types()
        {
            var type        = typeof(TestClass);
            var constructor = type.GetConstructors().Single();
            var parameter   = constructor.GetParameters().Single();
            var context     = CreateContext(parameter.ParameterType);

            var ex = Record.Exception(() => GeneratorFactory.GetGenerator(context));
            Assert.Null(ex);
        }

        private sealed class TestClass
        {
            public TestClass(in int value)
            {
            }
        }
    }

    public class RegisteredGenerator
        : AutoGeneratorsFixture
    {
        public static IEnumerable<object[]> GetRegisteredTypes()
        {
            return GeneratorFactory.Generators.Select(g => new object[] { g.Key, });
        }

        [SuppressMessage("Design", "CA1024:Use properties where appropriate")]
        public static IEnumerable<object[]> GetDataSetAndDataTableTypes()
        {
            yield return new object[] { typeof(DataSet), typeof(DataSetGenerator), };
            yield return new object[] { typeof(DataSetGeneratorFacet.TypedDataSet), typeof(DataSetGenerator), };
            yield return new object[] { typeof(DataTable), typeof(DataTableGenerator), };
            yield return new object[] { typeof(DataTableGeneratorFacet.TypedDataTable1), typeof(DataTableGenerator), };
            yield return new object[] { typeof(DataTableGeneratorFacet.TypedDataTable2), typeof(DataTableGenerator), };
        }

        [Theory]
        [MemberData(nameof(GetRegisteredTypes))]
        public void Generate_Should_Return_Value(Type type)
        {
            var generator = GeneratorFactory.Generators[type];

            Assert.IsType(type, InvokeGenerator(type, generator));
        }

        [Theory]
        [MemberData(nameof(GetRegisteredTypes))]
        public void GetGenerator_Should_Return_Generator(Type type)
        {
            var context   = CreateContext(type);
            var generator = GeneratorFactory.Generators[type];

            Assert.Equal(generator, GeneratorFactory.GetGenerator(context));
        }

        [Theory]
        [MemberData(nameof(GetDataSetAndDataTableTypes))]
        public void GetGenerator_Should_Return_Generator_For_DataSets_And_DataTables(Type dataType, Type generatorType)
        {
            var context = CreateContext(dataType);

            var generator = GeneratorFactory.GetGenerator(context);

            Assert.IsAssignableFrom(generatorType, generator);
        }
    }

    public class ExpandoObjectGenerator
        : AutoGeneratorsFixture
    {
        [Fact]
        public void Generate_Should_Return_Value()
        {
            var type      = typeof(ExpandoObject);
            var generator = new Generators.ExpandoObjectGenerator();

            dynamic instance = new ExpandoObject();
            dynamic child    = new ExpandoObject();

            instance.Property = string.Empty;
            instance.Child    = child;

            child.Property = 0;

            InvokeGenerator(type, generator, instance);

            string property      = instance.Property;
            int    childProperty = instance.Child.Property;

            Assert.NotEmpty(property);
            Assert.NotEqual(0, childProperty);
        }

        [Fact]
        public void GetGenerator_Should_Return_NullableGenerator()
        {
            var type    = typeof(ExpandoObject);
            var context = CreateContext(type);

            Assert.IsType<Generators.ExpandoObjectGenerator>(GeneratorFactory.GetGenerator(context));
        }
    }

    public class ArrayGenerator : AutoGeneratorsFixture
    {
        [Theory]
        [InlineData(typeof(TestEnum[]))]
        [InlineData(typeof(TestStruct[]))]
        [InlineData(typeof(TestClass[]))]
        [InlineData(typeof(ITestInterface[]))]
        [InlineData(typeof(TestAbstractClass[]))]
        public void Generate_Should_Return_Array(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var itemType  = type.GetElementType()!;
            var generator = CreateGenerator(typeof(ArrayGenerator<>), itemType);
            var array     = (Array)InvokeGenerator(type, generator);

            Assert.NotNull(array);
            foreach (var value in array)
            {
                Assert.NotNull(value);
            }
        }

        [Theory]
        [InlineData(typeof(TestEnum[]))]
        [InlineData(typeof(TestStruct[]))]
        [InlineData(typeof(TestClass[]))]
        [InlineData(typeof(ITestInterface[]))]
        [InlineData(typeof(TestAbstractClass[]))]
        public void GetGenerator_Should_Return_ArrayGenerator(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var context       = CreateContext(type);
            var itemType      = type.GetElementType()!;
            var generatorType = GetGeneratorType(typeof(ArrayGenerator<>), itemType);

            Assert.IsType(generatorType, GeneratorFactory.GetGenerator(context));
        }
    }

    public class EnumGenerator
        : AutoGeneratorsFixture
    {
        [Fact]
        public void Generate_Should_Return_Enum()
        {
            var type      = typeof(TestEnum);
            var generator = new EnumGenerator<TestEnum>();

            Assert.IsType<TestEnum>(InvokeGenerator(type, generator));
        }

        [Fact]
        public void GetGenerator_Should_Return_EnumGenerator()
        {
            var type    = typeof(TestEnum);
            var context = CreateContext(type);

            Assert.IsType<EnumGenerator<TestEnum>>(GeneratorFactory.GetGenerator(context));
        }
    }

    public class DictionaryGenerator
        : AutoGeneratorsFixture
    {
        [Theory]
        [InlineData(typeof(IDictionary<int, TestEnum>))]
        [InlineData(typeof(IDictionary<int, TestStruct>))]
        [InlineData(typeof(IDictionary<int, TestClass>))]
        [InlineData(typeof(IDictionary<int, ITestInterface>))]
        [InlineData(typeof(IDictionary<int, TestAbstractClass>))]
        [InlineData(typeof(Dictionary<int, TestClass>))]
        [InlineData(typeof(SortedList<int, TestClass>))]
        public void Generate_Should_Return_Dictionary(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var genericTypes = type.GetGenericArguments();
            var keyType      = genericTypes[0];
            var valueType    = genericTypes[1];
            var generator    = CreateGenerator(typeof(DictionaryGenerator<,>), keyType, valueType);
            var dictionary   = (IDictionary)InvokeGenerator(type, generator);

            Assert.NotNull(dictionary);

            foreach (var key in dictionary.Keys)
            {
                Assert.NotNull(key);
                var value = dictionary[key];
                Assert.NotNull(value);

                Assert.IsType(keyType, key);
                Assert.IsType(valueType, value);
            }
        }

        [Theory]
        [InlineData(typeof(IDictionary<int, TestEnum>))]
        [InlineData(typeof(IDictionary<int, TestStruct>))]
        [InlineData(typeof(IDictionary<int, TestClass>))]
        [InlineData(typeof(IDictionary<int, ITestInterface>))]
        [InlineData(typeof(IDictionary<int, TestAbstractClass>))]
        [InlineData(typeof(Dictionary<int, TestClass>))]
        [InlineData(typeof(SortedList<int, TestClass>))]
        public void GetGenerator_Should_Return_DictionaryGenerator(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var context       = CreateContext(type);
            var genericTypes  = type.GetGenericArguments();
            var keyType       = genericTypes[0];
            var valueType     = genericTypes[1];
            var generatorType = GetGeneratorType(typeof(DictionaryGenerator<,>), keyType, valueType);

            Assert.IsType(generatorType, GeneratorFactory.GetGenerator(context));
        }
    }

    public class ListGenerator
        : AutoGeneratorsFixture
    {
        [Theory]
        [InlineData(typeof(IList<TestEnum>))]
        [InlineData(typeof(IList<TestStruct>))]
        [InlineData(typeof(IList<TestClass>))]
        [InlineData(typeof(IList<ITestInterface>))]
        [InlineData(typeof(IList<TestAbstractClass>))]
        [InlineData(typeof(ICollection<TestEnum>))]
        [InlineData(typeof(ICollection<TestStruct>))]
        [InlineData(typeof(ICollection<TestClass>))]
        [InlineData(typeof(ICollection<ITestInterface>))]
        [InlineData(typeof(ICollection<TestAbstractClass>))]
        [InlineData(typeof(List<TestClass>))]
        public void Generate_Should_Return_List(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var genericTypes = type.GetGenericArguments();
            var itemType     = genericTypes[0];
            var generator    = CreateGenerator(typeof(ListGenerator<>), itemType);
            var list         = (IEnumerable)InvokeGenerator(type, generator);

            Assert.NotNull(list);
            foreach (var item in list)
            {
                Assert.NotNull(item);
            }
        }

        [Theory]
        [InlineData(typeof(IList<TestEnum>))]
        [InlineData(typeof(IList<TestStruct>))]
        [InlineData(typeof(IList<TestClass>))]
        [InlineData(typeof(IList<ITestInterface>))]
        [InlineData(typeof(IList<TestAbstractClass>))]
        [InlineData(typeof(ICollection<TestEnum>))]
        [InlineData(typeof(ICollection<TestStruct>))]
        [InlineData(typeof(ICollection<TestClass>))]
        [InlineData(typeof(ICollection<ITestInterface>))]
        [InlineData(typeof(ICollection<TestAbstractClass>))]
        [InlineData(typeof(List<TestClass>))]
        public void GetGenerator_Should_Return_ListGenerator(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var context       = CreateContext(type);
            var genericTypes  = type.GetGenericArguments();
            var itemType      = genericTypes[0];
            var generatorType = GetGeneratorType(typeof(ListGenerator<>), itemType);

            Assert.IsType(generatorType, GeneratorFactory.GetGenerator(context));
        }
    }

    public class SetGenerator
        : AutoGeneratorsFixture
    {
        [Theory]
        [InlineData(typeof(ISet<TestEnum>))]
        [InlineData(typeof(ISet<TestStruct>))]
        [InlineData(typeof(ISet<TestClass>))]
        [InlineData(typeof(ISet<ITestInterface>))]
        [InlineData(typeof(ISet<TestAbstractClass>))]
        [InlineData(typeof(HashSet<TestClass>))]
        public void Generate_Should_Return_Set(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var genericTypes = type.GetGenericArguments();
            var itemType     = genericTypes[0];
            var generator    = CreateGenerator(typeof(SetGenerator<>), itemType);
            var set          = (IEnumerable)InvokeGenerator(type, generator);

            Assert.NotNull(set);
            foreach (var value in set)
            {
                Assert.NotNull(value);
            }
        }

        [Theory]
        [InlineData(typeof(ISet<TestEnum>))]
        [InlineData(typeof(ISet<TestStruct>))]
        [InlineData(typeof(ISet<TestClass>))]
        [InlineData(typeof(ISet<ITestInterface>))]
        [InlineData(typeof(ISet<TestAbstractClass>))]
        [InlineData(typeof(HashSet<TestClass>))]
        public void GetGenerator_Should_Return_SetGenerator(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var context       = CreateContext(type);
            var genericTypes  = type.GetGenericArguments();
            var itemType      = genericTypes[0];
            var generatorType = GetGeneratorType(typeof(SetGenerator<>), itemType);

            Assert.IsType(generatorType, GeneratorFactory.GetGenerator(context));
        }
    }

    public class EnumerableGenerator
        : AutoGeneratorsFixture
    {
        [Theory]
        [InlineData(typeof(IEnumerable<TestEnum>))]
        [InlineData(typeof(IEnumerable<TestStruct>))]
        [InlineData(typeof(IEnumerable<TestClass>))]
        [InlineData(typeof(IEnumerable<ITestInterface>))]
        [InlineData(typeof(IEnumerable<TestAbstractClass>))]
        public void Generate_Should_Return_Enumerable(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var genericTypes = type.GetGenericArguments();
            var itemType     = genericTypes[0];
            var generator    = CreateGenerator(typeof(EnumerableGenerator<>), itemType);
            var enumerable   = (IEnumerable)InvokeGenerator(type, generator);

            Assert.NotNull(enumerable);
            foreach (var value in enumerable)
            {
                Assert.NotNull(value);
            }
        }

        [Theory]
        [InlineData(typeof(IEnumerable<TestEnum>))]
        [InlineData(typeof(IEnumerable<TestStruct>))]
        [InlineData(typeof(IEnumerable<TestClass>))]
        [InlineData(typeof(IEnumerable<ITestInterface>))]
        [InlineData(typeof(IEnumerable<TestAbstractClass>))]
        public void GetGenerator_Should_Return_EnumerableGenerator(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var context       = CreateContext(type);
            var genericTypes  = type.GetGenericArguments();
            var itemType      = genericTypes[0];
            var generatorType = GetGeneratorType(typeof(EnumerableGenerator<>), itemType);

            Assert.IsType(generatorType, GeneratorFactory.GetGenerator(context));
        }
    }

    public class NullableGenerator
        : AutoGeneratorsFixture
    {
        [Fact]
        public void Generate_Should_Return_Value()
        {
            var type      = typeof(TestEnum?);
            var generator = new NullableGenerator<TestEnum>();

            Assert.IsType<TestEnum>(InvokeGenerator(type, generator));
        }

        [Fact]
        public void GetGenerator_Should_Return_NullableGenerator()
        {
            var type    = typeof(TestEnum?);
            var context = CreateContext(type);

            Assert.IsType<NullableGenerator<TestEnum>>(GeneratorFactory.GetGenerator(context));
        }
    }

    public class TypeGenerator
        : AutoGeneratorsFixture
    {
        [Theory]
        [InlineData(typeof(TestStruct))]
        [InlineData(typeof(TestClass))]
        [InlineData(typeof(ITestInterface))]
        [InlineData(typeof(TestAbstractClass))]
        public void Generate_Should_Return_Value(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var generator = CreateGenerator(typeof(TypeGenerator<>), type);

            if (type.IsInterface || type.IsAbstract)
            {
                Assert.Null(InvokeGenerator(type, generator));
            }
            else
            {
                Assert.IsAssignableFrom(type, InvokeGenerator(type, generator));
            }
        }

        [Theory]
        [InlineData(typeof(TestStruct))]
        [InlineData(typeof(TestClass))]
        [InlineData(typeof(ITestInterface))]
        [InlineData(typeof(TestAbstractClass))]
        public void GetGenerator_Should_Return_TypeGenerator(Type type)
        {
            var context       = CreateContext(type);
            var generatorType = GetGeneratorType(typeof(TypeGenerator<>), type);

            Assert.IsType(generatorType, GeneratorFactory.GetGenerator(context));
        }
    }

    public class GeneratorOverrides
        : AutoGeneratorsFixture
    {
        private readonly AutoGeneratorOverride _generatorOverride;

        private HashSet<AutoGeneratorOverride> _overrides;

        public GeneratorOverrides()
        {
            _generatorOverride = new TestGeneratorOverride(true);
            _overrides = new HashSet<AutoGeneratorOverride>
            {
                new TestGeneratorOverride(),
                _generatorOverride,
            };
        }

        [Fact]
        public void Should_Return_Generator_If_No_Matching_Override()
        {
            _overrides = new HashSet<AutoGeneratorOverride>
            {
                new TestGeneratorOverride(),
            };

            var context = CreateContext(typeof(int), _overrides);
            Assert.IsType<IntGenerator>(GeneratorFactory.GetGenerator(context));
        }

        [Fact]
        public void Should_Invoke_Generator()
        {
            var context           = CreateContext(typeof(string), _overrides);
            var generatorOverride = GeneratorFactory.GetGenerator(context);

            var value = generatorOverride.Generate(context);
            Assert.NotNull(value);
            Assert.IsType<string>(value);
        }

        private class TestGeneratorOverride : AutoGeneratorOverride
        {
            public TestGeneratorOverride(bool shouldOverride = false)
            {
                ShouldOverride = shouldOverride;
            }

            public bool ShouldOverride { get; }

            public override bool CanOverride(AutoGenerateContext context)
            {
                return ShouldOverride;
            }

            public override void Generate(AutoGenerateOverrideContext context)
            {
            }
        }
    }
}
