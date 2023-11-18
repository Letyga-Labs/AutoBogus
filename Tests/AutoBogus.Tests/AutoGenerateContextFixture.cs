using System.Diagnostics.CodeAnalysis;
using AutoBogus.Generation;
using Bogus;
using Xunit;

namespace AutoBogus.Tests;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class AutoGenerateContextFixture
{
    private readonly AutoConfig          _config   = new();
    private readonly Faker               _faker    = new();
    private readonly IEnumerable<string> _ruleSets = Enumerable.Empty<string>();

    private AutoGenerateContext _context = null!;

    public class GenerateMany_Internal : AutoGenerateContextFixture
    {
        private readonly List<int> _items;

        private int _value;

        public GenerateMany_Internal()
        {
            _value = _faker.Random.Int();
            _items = new List<int> { _value };
            _context = new AutoGenerateContext(_faker, _config)
            {
                RuleSets = _ruleSets,
            };
        }

        [Fact]
        public void Should_Generate_Configured_RepeatCount()
        {
            var count    = _faker.Random.Int(3, 5);
            var expected = Enumerable.Range(0, count).Select(i => _value).ToList();

            _config.RepeatCount = _ => count;

            Generator.GenerateMany(_context, _items, false, null, 1, () => _value);

            Assert.Equal(expected, _items);
        }

        [Fact]
        public void Should_Generate_Duplicates_If_Not_Unique()
        {
            Generator.GenerateMany(_context, _items, false, 2, 1, () => _value);

            var expected = new List<int>
            {
                _value,
                _value,
            };

            Assert.Equal(expected, _items);
        }

        [Fact]
        public void Should_Not_Generate_Duplicates_If_Unique()
        {
            var first  = _value;
            var second = _faker.Random.Int();

            Generator.GenerateMany(_context, _items, true, 2, 1, () =>
            {
                var item = _value;
                _value = second;

                return item;
            });

            var expected = new List<int>
            {
                first,
                second,
            };

            Assert.Equal(expected, _items);
        }

        [Fact]
        public void Should_Short_Circuit_If_Unique_Attempts_Overflow()
        {
            var attempts = 0;

            Generator.GenerateMany(_context, _items, true, 2, 1, () =>
            {
                attempts++;
                return _value;
            });

            Assert.Equal(AutoConfig.GenerateAttemptsThreshold, attempts);

            var expected = new List<int> { _value };

            Assert.Equal(expected, _items);
        }
    }
}
