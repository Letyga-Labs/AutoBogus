using System.Diagnostics.CodeAnalysis;
using AutoBogus.NSubstitute;
using Bogus;
using Xunit;

namespace AutoBogus.Tests;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class AutoConfigBuilderFixture
{
    private readonly AutoConfigBuilder _builder;
    private readonly AutoConfig        _config;
    private readonly Faker             _faker;

    public AutoConfigBuilderFixture()
    {
        _faker   = new Faker();
        _config  = new AutoConfig();
        _builder = new AutoConfigBuilder(_config);
    }

    private interface ITestBuilder
    {
    }

    public class WithLocale
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Set_Config_Locale()
        {
            var locale = _faker.Random.String();
            _builder.WithLocale<ITestBuilder>(locale, null!);
            Assert.Equal(locale, _config.Locale);
        }

        [Fact]
        public void Should_Set_Config_Locale_To_Default_If_Null()
        {
            _config.Locale = _faker.Random.String();
            _builder.WithLocale<ITestBuilder>(null, null!);
            Assert.Equal(AutoConfig.DefaultLocale, _config.Locale);
        }
    }

    public class WithRepeatCount
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Set_Config_RepeatCount()
        {
            var count = _faker.Random.Int();
            _builder.WithRepeatCount<ITestBuilder>(_ => count, null!);
            Assert.Equal(count, _config.RepeatCount.Invoke(null!));
        }

        [Fact]
        public void Should_Set_Config_RepeatCount_To_Default_If_Null()
        {
            var count = AutoConfig.DefaultRepeatCount.Invoke(null!);
            _builder.WithRepeatCount<ITestBuilder>(null, null!);
            Assert.Equal(count, _config.RepeatCount.Invoke(null!));
        }
    }

    public class WithRecursiveDepth
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Set_Config_RecursiveDepth()
        {
            var depth = _faker.Random.Int();
            _builder.WithRecursiveDepth<ITestBuilder>(_ => depth, null!);
            Assert.Equal(depth, _config.RecursiveDepth.Invoke(null!));
        }

        [Fact]
        public void Should_Set_Config_RecursiveDepth_To_Default_If_Null()
        {
            var depth = AutoConfig.DefaultRecursiveDepth.Invoke(null!);
            _builder.WithRecursiveDepth<ITestBuilder>(null, null!);
            Assert.Equal(depth, _config.RecursiveDepth.Invoke(null!));
        }
    }

    public class WithTreeDepth
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Set_Config_TreeDepth()
        {
            var depth = _faker.Random.Int();
            _builder.WithTreeDepth<ITestBuilder>(_ => depth, null!);
            Assert.Equal(depth, _config.TreeDepth.Invoke(null!));
        }

        [Fact]
        public void Should_Set_Config_TreeDepth_To_Default_If_Null()
        {
            var depth = AutoConfig.DefaultTreeDepth.Invoke(null!);
            _builder.WithTreeDepth<ITestBuilder>(null, null!);
            Assert.Equal(depth, _config.TreeDepth.Invoke(null!));
        }
    }

    public class WithBinder
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Set_Config_Binder()
        {
            var binder = new NSubstituteBinder();
            _builder.WithBinder<ITestBuilder>(binder, null!);
            Assert.Equal(binder, _config.Binder);
        }

        [Fact]
        public void Should_Set_Config_Binder_To_Default_If_Null()
        {
            _config.Binder = new NSubstituteBinder();
            _builder.WithBinder<ITestBuilder>(null, null!);
            Assert.IsType<AutoBinder>(_config.Binder);
        }
    }

    public class WithFakerHub
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Set_Config_FakerHub()
        {
            var fakerHub = new Faker();
            _builder.WithFakerHub<ITestBuilder>(fakerHub, null!);
            Assert.Equal(fakerHub, _config.FakerHub);
        }
    }

    public class WithSkip_Type
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Not_Add_Type_If_Already_Added()
        {
            var type1 = typeof(int);
            var type2 = typeof(int);

            _config.SkipTypes.Add(type1);
            _builder.WithSkip<ITestBuilder>(type2, null!);
            Assert.Single(_config.SkipTypes);
        }

        [Fact]
        public void Should_Add_Type_To_Skip()
        {
            var type1 = typeof(int);
            var type2 = typeof(string);

            _config.SkipTypes.Add(type1);
            _builder.WithSkip<ITestBuilder>(type2, null!);

            var expected = new HashSet<Type>
            {
                type1,
                type2,
            };
            Assert.Equal(expected, _config.SkipTypes);
        }
    }

    public class WithSkip_TypePath
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Not_Add_Member_If_Already_Added()
        {
            var type   = typeof(TestSkip);
            var member = $"{type.FullName}.Value";

            _config.SkipPaths.Add(member);
            _builder.WithSkip<ITestBuilder>(type, "Value", null!);
            Assert.Single(_config.SkipPaths);
        }

        [Fact]
        public void Should_Add_MemberName_To_Skip()
        {
            var type = typeof(TestSkip);
            var path = _faker.Random.String();

            _config.SkipPaths.Add(path);
            _builder.WithSkip<ITestBuilder>(type, "Value", null!);

            var expected = new HashSet<string>
            {
                path,
                $"{type.FullName}.Value",
            };
            Assert.Equal(expected, _config.SkipPaths);
        }

        private sealed class TestSkip
        {
            public string Value { get; set; } = null!;
        }
    }

    public class WithSkip_Path
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Not_Add_Member_If_Already_Added()
        {
            var type   = typeof(TestSkip);
            var member = $"{type.FullName}.Value";

            _config.SkipPaths.Add(member);
            _builder.WithSkip<ITestBuilder, TestSkip>("Value", null!);
            Assert.Single(_config.SkipPaths);
        }

        [Fact]
        public void Should_Add_MemberName_To_Skip()
        {
            var type = typeof(TestSkip);
            var path = _faker.Random.String();

            _config.SkipPaths.Add(path);
            _builder.WithSkip<ITestBuilder, TestSkip>("Value", null!);

            var expected = new HashSet<string>
            {
                path,
                $"{type.FullName}.Value",
            };
            Assert.Equal(expected, _config.SkipPaths);
        }

        private sealed class TestSkip
        {
            public string Value { get; set; } = null!;
        }
    }

    public class WithOverride
        : AutoConfigBuilderFixture
    {
        [Fact]
        public void Should_Not_Add_Null_Override()
        {
            _builder.WithOverride<ITestBuilder>(null, null!);
            Assert.Empty(_config.Overrides);
        }

        [Fact]
        public void Should_Not_Add_Override_If_Already_Added()
        {
            var generatorOverride = new TestGeneratorOverride();

            _config.Overrides.Add(generatorOverride);
            _builder.WithOverride<ITestBuilder>(generatorOverride, null!);
            Assert.Single(_config.Overrides);
        }

        [Fact]
        public void Should_Add_Override_If_Equivalency_Is_Different()
        {
            var generatorOverride1 = new TestGeneratorOverride();
            var generatorOverride2 = new TestGeneratorOverride();

            _config.Overrides.Add(generatorOverride1);
            _builder.WithOverride<ITestBuilder>(generatorOverride2, null!);

            var expected = new HashSet<AutoGeneratorOverride>
            {
                generatorOverride1,
                generatorOverride2,
            };

            Assert.Equal(expected, _config.Overrides);
        }

        private class TestGeneratorOverride
            : AutoGeneratorOverride
        {
            public override bool CanOverride(AutoGenerateContext context)
            {
                return false;
            }

            public override void Generate(AutoGenerateOverrideContext context)
            {
            }
        }
    }
}
