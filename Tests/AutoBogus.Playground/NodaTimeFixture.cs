using Bogus;
using FluentAssertions;
using Xunit;

namespace AutoBogus.Playground;

public static class NodaTimeFixture
{
    public sealed class TimeHolder
    {
        private DateTime _time;
        private bool     _unstable;

        public DateTime Time
        {
            get => _time;
            set
            {
                if (value.Day != 2 && value.Month != 2)
                {
                    _unstable = true;
                }

                if (_unstable)
                {
                    throw new InvalidOperationException();
                }

                _time = value;
            }
        }
    }

    public class TestValidValueCreation
    {
        private readonly DateTime _validDate = new(2020, 2, 2, 0, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void TestFaker()
        {
            var created = new Faker<TimeHolder>()
                .RuleFor(x => x.Time, _ => _validDate)
                .Generate();

            created.Should().NotBeNull();
        }

        [Fact]
        public void TestAutoFaker()
        {
            var fake = new AutoFaker<TimeHolder>().RuleFor(x => x.Time, _ => _validDate);
            fake.Generate();
            var created = fake.Generate();

            created.Should().NotBeNull();
        }
    }
}
