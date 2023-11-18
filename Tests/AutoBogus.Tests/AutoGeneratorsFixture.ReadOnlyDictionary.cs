using System.Collections.ObjectModel;
using AutoBogus.Generators;
using AutoBogus.Tests.Models.Simple;
using Xunit;

namespace AutoBogus.Tests;

public partial class AutoGeneratorsFixture
{
    public class ReadOnlyDictionaryGenerator
        : AutoGeneratorsFixture
    {
        [Theory]
        [InlineData(typeof(IReadOnlyDictionary<int, string>))]
        [InlineData(typeof(ReadOnlyDictionary<int, string>))]
        public void Generate_Should_Return_Dictionary(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var genericTypes = type.GetGenericArguments();
            var keyType      = genericTypes[0];
            var valueType    = genericTypes[1];
            var generator    = CreateGenerator(typeof(ReadOnlyDictionaryGenerator<,>), keyType, valueType);
            var dictionary   = (IReadOnlyDictionary<int, string>)InvokeGenerator(type, generator);

            Assert.NotNull(dictionary);
            Assert.All(dictionary, it => Assert.NotNull(it.Value));

            foreach (var key in dictionary.Keys)
            {
                var value = dictionary[key];

                Assert.IsType(keyType, key);
                Assert.IsType(valueType, value);
            }
        }

        [Theory]
        [InlineData(typeof(IReadOnlyDictionary<int, TestEnum>))]
        [InlineData(typeof(IReadOnlyDictionary<int, TestStruct>))]
        [InlineData(typeof(IReadOnlyDictionary<int, TestClass>))]
        [InlineData(typeof(IReadOnlyDictionary<int, ITestInterface>))]
        [InlineData(typeof(IReadOnlyDictionary<int, TestAbstractClass>))]
        [InlineData(typeof(ReadOnlyDictionary<int, TestClass>))]
        public void GetGenerator_Should_Return_ReadOnlyDictionaryGenerator(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            var context       = CreateContext(type);
            var genericTypes  = type.GetGenericArguments();
            var keyType       = genericTypes[0];
            var valueType     = genericTypes[1];
            var generatorType = GetGeneratorType(typeof(ReadOnlyDictionaryGenerator<,>), keyType, valueType);

            var generator = AutoGeneratorFactory.GetGenerator(context);

            Assert.IsType(generatorType, generator);
        }
    }
}
