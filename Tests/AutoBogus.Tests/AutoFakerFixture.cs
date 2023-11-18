using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Reflection;
using AutoBogus.Tests.Models;
using AutoBogus.Tests.Models.Complex;
using AutoBogus.Tests.Models.Simple;
using Bogus;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AutoBogus.Tests;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class AutoFakerFixture
{
    private const string _name = "Generate";

    private static readonly Type _type = typeof(AutoFaker);

    [SuppressMessage("Design", "CA1024:Use properties where appropriate")]
    public static IEnumerable<object[]> GetTypes()
    {
        foreach (var type in AutoGeneratorFactory.Generators.Keys)
        {
            yield return new object[] { type, };
        }

        yield return new object[] { typeof(string[]), };
        yield return new object[] { typeof(TestEnum), };
        yield return new object[] { typeof(IDictionary<Guid, TestStruct>), };
        yield return new object[] { typeof(IEnumerable<TestClass>), };
        yield return new object[] { typeof(int?), };
    }

    public static void AssertGenerate(Type type, MethodInfo methodInfo, IAutoFaker? faker, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        var method   = methodInfo.MakeGenericMethod(type);
        var instance = method.Invoke(faker, args);

        instance.Should().BeGenerated();
    }

    public static void AssertGenerateMany(Type type, MethodInfo methodInfo, IAutoFaker? faker, params object[] args)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        var count     = AutoConfig.DefaultRepeatCount.Invoke(null!);
        var method    = methodInfo.MakeGenericMethod(type);
        var instances = (IEnumerable)method.Invoke(faker, args)!;

        var list = new List<object>();
        foreach (var instance in instances)
        {
            instance.Should().BeGenerated();
            list.Add(instance);
        }

        Assert.Equal(count, list.Count);
    }

    public static void AssertGenerateMany(IEnumerable<Order> instances)
    {
        ArgumentNullException.ThrowIfNull(instances);

        var count = AutoConfig.DefaultRepeatCount.Invoke(null!);
        var list  = new List<Order>();
        foreach (var instance in instances)
        {
            instance.Should().BeGeneratedWithoutMocks();
            list.Add(instance);
        }

        Assert.Equal(count, list.Count);
    }

    private Action<TBuilder> CreateConfigure<TBuilder>(AutoConfig assertConfig, Action<TBuilder>? configure = null)
    {
        return builder =>
        {
            configure?.Invoke(builder);
            var instance = builder as AutoConfigBuilder;
            Assert.NotEqual(assertConfig, instance!.Config);
        };
    }

    public class Configure : AutoFakerFixture
    {
        [Fact]
        public void Should_Configure_Default_Config()
        {
            AutoConfig? config = null;
            AutoFaker.Configure(builder =>
            {
                config = ((AutoConfigBuilder)builder).Config;
            });

            Assert.Equal(AutoFaker.DefaultConfig, config);
        }
    }

    public class Create
        : AutoFakerFixture
    {
        [Fact]
        public void Should_Configure_Child_Config()
        {
            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(AutoFaker.DefaultConfig);
            Assert.IsType<AutoFaker>(AutoFaker.Create(configure));
        }
    }

    public class Generate_Instance : AutoFakerFixture
    {
        private static readonly Type   _interfaceType = typeof(IAutoFaker);
        private static readonly string _methodName    = $"{_interfaceType.FullName}.{_name}";

        private static readonly MethodInfo _generate = _type.GetMethod(
            _methodName,
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(Action<IAutoGenerateConfigBuilder>), },
            null)!;

        private static readonly MethodInfo _generateMany = _type.GetMethod(
            _methodName,
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(int), typeof(Action<IAutoGenerateConfigBuilder>), },
            null)!;

        private readonly AutoConfig _config;

        private readonly IAutoFaker _faker;

        public Generate_Instance()
        {
            var faker = (AutoFaker)AutoFaker.Create();

            _faker  = faker;
            _config = faker.Config;
        }

        [Theory]
        [MemberData(nameof(GetTypes))]
        public void Should_Generate_Type(Type type)
        {
            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(_config);
            AssertGenerate(type, _generate, _faker, configure);
        }

        [Theory]
        [MemberData(nameof(GetTypes))]
        public void Should_Generate_Many_Types(Type type)
        {
            var count     = AutoConfig.DefaultRepeatCount.Invoke(null!);
            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(_config);

            AssertGenerateMany(type, _generateMany, _faker, count, configure);
        }

        [Fact]
        public void Should_Generate_Complex_Type()
        {
            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(_config);
            _faker.Generate<Order>(configure).Should().BeGeneratedWithoutMocks();
        }

        [Fact]
        public void Should_Generate_Many_Complex_Types()
        {
            var count     = AutoConfig.DefaultRepeatCount.Invoke(null!);
            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(_config);
            var instances = _faker.Generate<Order>(count, configure);

            AssertGenerateMany(instances);
        }
    }

    public class Generate_Static
        : AutoFakerFixture
    {
        private static readonly MethodInfo _generate = _type.GetMethod(
            _name,
            BindingFlags.Static | BindingFlags.Public,
            null,
            new[] { typeof(Action<IAutoGenerateConfigBuilder>), },
            null)!;

        private static readonly MethodInfo _generateMany = _type.GetMethod(
            _name,
            BindingFlags.Static | BindingFlags.Public,
            null,
            new[] { typeof(int), typeof(Action<IAutoGenerateConfigBuilder>), },
            null)!;

        [Theory]
        [MemberData(nameof(GetTypes))]
        public void Should_Generate_Type(Type type)
        {
            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(AutoFaker.DefaultConfig);
            AssertGenerate(type, _generate, null, configure);
        }

        [Theory]
        [MemberData(nameof(GetTypes))]
        public void Should_Generate_Many_Types(Type type)
        {
            var count     = AutoConfig.DefaultRepeatCount.Invoke(null!);
            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(AutoFaker.DefaultConfig);

            AssertGenerateMany(type, _generateMany, null, count, configure);
        }

        [Fact]
        public void Should_Generate_Complex_Type()
        {
            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(AutoFaker.DefaultConfig);
            AutoFaker.Generate<Order>(configure).Should().BeGeneratedWithoutMocks();
        }

        [Fact]
        public void Should_Generate_Many_Complex_Types()
        {
            var count     = AutoConfig.DefaultRepeatCount.Invoke(null!);
            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(AutoFaker.DefaultConfig);
            var instances = AutoFaker.Generate<Order>(count, configure);

            AssertGenerateMany(instances);
        }
    }

    public class AutoFaker_T
        : AutoFakerFixture
    {
        private readonly Faker<Order> _faker = new AutoFaker<Order>();

        [Fact]
        public void Should_Generate_Type()
        {
            _faker.Generate().Should().BeGeneratedWithoutMocks();
        }

        [Fact]
        public void Should_Populate_ExpandoObject()
        {
            var faker = new AutoFaker<dynamic>();

            dynamic instance = new ExpandoObject();
            dynamic child    = new ExpandoObject();

            instance.Property = string.Empty;
            instance.Child    = child;

            child.Property = 0;

            faker.Populate(instance);

            string property      = instance.Property;
            int    childProperty = instance.Child.Property;

            Assert.NotEmpty(property);
            Assert.NotEqual(0, childProperty);
        }

        [Fact]
        public void Should_Populate_Instance()
        {
            var faker      = new Faker();
            var id         = faker.Random.Int();
            var calculator = Substitute.For<ICalculator>();
            var order      = new Order(id, calculator);

            _faker.Populate(order);

            order.Should().BeGeneratedWithMocks();
            Assert.Equal(id, order.Id);
            Assert.Equal(calculator, order.Calculator);
        }

        [Fact]
        public void Should_Use_Custom_Instantiator()
        {
            var binder = Substitute.For<IAutoBinder>();
            binder.GetMembers(typeof(Order)).Returns(new Dictionary<string, MemberInfo>());

            new AutoFaker<Order>(null, binder)
                .CustomInstantiator(_ => new Order(default, default))
                .Generate();

            binder.DidNotReceive().CreateInstance<Order>(Arg.Any<AutoGenerateContext>());
        }

        [Fact]
        public void Should_Not_Generate_Rule_Set_Members()
        {
            var code = Guid.NewGuid();
            var order = _faker
                .RuleFor(o => o.Code, code)
                .Generate();

            order.Should().BeGeneratedWithoutMocks();
            Assert.Equal(code, order.Code);
        }

        [Fact]
        public void Should_Not_Generate_If_No_Default_Rule_Set()
        {
            _faker.RuleSet("test", rules =>
            {
                // No default constructor so ensure a create action is defined
                // Make the values default so the NotBeGenerated() check passes
                rules.CustomInstantiator(f => new Order(default, default));
            });

            _faker.Generate("test").Should().NotBeGenerated();
        }

        [Fact]
        public void Should_Call_FinishWith()
        {
            Order? instance = null;
            var order = new AutoFaker<Order>()
                .FinishWith((_, i) => instance = i)
                .Generate();

            order.Should().BeGeneratedWithoutMocks();
            Assert.Equal(order, instance);
        }

        [Fact]
        public void Should_Not_Initialize_Properties_Twice()
        {
            // Arrange
            var random1 = new Randomizer(12345);
            var random2 = new Randomizer(12345);

            var faker = new Faker
            {
                Random = random1,
            };

            var autoFaker = new AutoFaker<TestClassWithSingleProperty<int>>();

            autoFaker.Configure(
                builder => builder.WithFakerHub(faker));

            // Act
            var instance = autoFaker.Generate(); // Should pull one int from random1

            var expectedValue = random2.Int();

            // Assert
            Assert.Equal(expectedValue, instance.Value);
        }
    }

    public class AutoFaker_WithFakerHub
        : AutoFakerFixture
    {
        [Fact]
        public void Should_Use_Caller_Supplied_FakerHub()
        {
            // We infer that our FakerHub was used by reseeding it and testing that we get the same sequence both times.
            var fakerHub = new Faker();

            var configure = CreateConfigure<IAutoGenerateConfigBuilder>(
                AutoFaker.DefaultConfig,
                builder => builder.WithFakerHub(fakerHub));

            var faker = AutoFaker.Create(configure);

            fakerHub.Random = new Randomizer(1);

            var instance1 = faker.Generate<TestObject>();

            fakerHub.Random = new Randomizer(1);

            var instance2 = faker.Generate<TestObject>();

            Assert.Equivalent(instance2, instance1);
        }

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private")]
        private class TestObject
        {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS0414 // Field is assigned but its value is never used
            public int    IntegerValue;
            public string StringValue = null!;
#pragma warning restore CS0414 // Field is assigned but its value is never used
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
        }
    }

    public class Behaviors_Skip
        : AutoFakerFixture
    {
        [Fact]
        public void Should_Skip_Configured_Types()
        {
            var instance = AutoFaker.Generate<Order>(builder =>
            {
                builder
                    .WithSkip<ICalculator>()
                    .WithSkip<Guid?>();
            });

            Assert.Null(instance.Calculator);
            Assert.Null(instance.Code);
        }

        [Fact]
        public void Should_Skip_Configured_Members()
        {
            var instance = AutoFaker.Generate<Order>(builder =>
            {
                builder
                    .WithSkip<Order>(o => o.Discounts)
                    .WithSkip<OrderItem>(i => i.Discounts);
            });

            Assert.Null(instance.Discounts);
            Assert.NotNull(instance.Items);
            Assert.All(instance.Items, it => Assert.Null(it.Discounts));
        }
    }

    public class Behaviors_Types
        : AutoFakerFixture
    {
        [Fact]
        public void Should_Not_Generate_Interface_Type()
        {
            Assert.Null(AutoFaker.Generate<ITestInterface>());
        }

        [Fact]
        public void Should_Not_Generate_Abstract_Class_Type()
        {
            Assert.Null(AutoFaker.Generate<TestAbstractClass>());
        }
    }

    public class Behaviors_Recursive
        : AutoFakerFixture
    {
        private readonly TestRecursiveClass _instance;

        public Behaviors_Recursive()
        {
            _instance = AutoFaker.Generate<TestRecursiveClass>(builder =>
            {
                builder.WithRecursiveDepth(3);
            });
        }

        [Fact]
        public void Should_Generate_Recursive_Types()
        {
            Assert.NotNull(_instance.Child);
            Assert.NotNull(_instance.Child.Child);
            Assert.NotNull(_instance.Child.Child.Child);
            Assert.Null(_instance.Child.Child.Child.Child);
        }

        [Fact]
        public void Should_Generate_Recursive_Lists()
        {
            var children  = _instance.Children;
            var children1 = children.SelectMany(c => c.Children).ToList();
            var children2 = children1.SelectMany(c => c.Children).ToList();
            var children3 = children2.Where(c => c.Children != null).ToList();

            Assert.Equal(3, children.Count());
            Assert.Equal(9, children1.Count);
            Assert.Equal(27, children2.Count);
            Assert.Empty(children3);
        }

        [Fact]
        public void Should_Generate_Recursive_Sub_Types()
        {
            Assert.NotNull(_instance.Sub);
            Assert.NotNull(_instance.Sub.Value.Sub);
            Assert.NotNull(_instance.Sub.Value.Sub.Value.Sub);
            Assert.Null(_instance.Sub.Value.Sub.Value.Sub.Value.Sub);
        }
    }

    private class TestFaker
        : AutoFaker<Order>
    {
    }
}
