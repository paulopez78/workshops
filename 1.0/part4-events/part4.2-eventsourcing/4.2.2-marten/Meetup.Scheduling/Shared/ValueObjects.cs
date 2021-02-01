
namespace Meetup.Scheduling.Shared
{
    public record PositiveNumber
    {
        public int Value { get; init; }

        PositiveNumber(int value) => Value = value > 0 ? value : 0;

        public static PositiveNumber From(int value) => new(value);

        public static implicit operator int(PositiveNumber number) => number.Value;
        public static implicit operator PositiveNumber(int number) => From(number);
    }
}