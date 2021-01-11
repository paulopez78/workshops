using System;
using System.Text.RegularExpressions;

namespace Meetup.Scheduling.Domain
{
    public record PositiveNumber
    {
        public int Value { get; init; }

        PositiveNumber(int value) => Value = value > 0 ? value : 0;

        public static PositiveNumber From(int value) => new(value);

        public static implicit operator int(PositiveNumber number) => number.Value;
        public static implicit operator PositiveNumber(int number) => From(number);
    }

    public record Address
    {
        public string Value { get; init; }

        Address(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            Value = value;
        }

        public static Address From(string address) => new(address);

        public static implicit operator string(Address address) => address.Value;
        public static implicit operator Address(string address) => From(address);
    }

    public record Location
    {
        public bool     IsOnline { get; init; }
        public Uri?     Url      { get; init; }
        public Address? Address  { get; init; }

        Location()
        {
        }

        Location(bool isOnline, Uri? url, Address? address)
        {
            switch (isOnline)
            {
                case true when url is null:
                    throw new ArgumentNullException(nameof(url));
                case false when address is null:
                    throw new ArgumentNullException(nameof(address));
            }

            IsOnline = isOnline;
            Url      = url;
            Address  = address;
        }

        public static Location OnSite(Address address) => new(false, null, address);
        public static Location Online(Uri url) => new(true, url, null);
    }

    public record DateTimeRange
    {
        public DateTimeOffset Start { get; init; }
        public DateTimeOffset End   { get; init; }

        DateTimeRange(DateTimeOffset start, DateTimeOffset end)
        {
            if (start > end)
                throw new ArgumentException("Incorrect date time range, end date must be after start date");

            Start = start;
            End   = end;
        }

        DateTimeRange(DateTimeOffset start, PositiveNumber durationinHours)
        {
            Start = start;
            End   = start.AddHours(durationinHours);
        }

        public static DateTimeRange From(DateTimeOffset start, DateTimeOffset end) => new(start, end);

        public static DateTimeRange From(DateTimeOffset start, PositiveNumber durationInHours) =>
            new(start, durationInHours);
    }

    public record GroupSlug
    {
        public string Value { get; init; }

        GroupSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            Regex regex = new(@"^[a-z\d](?:[a-z\d_-]*[a-z\d])?$");
            if (!regex.IsMatch(value))
                throw new ArgumentException($"{nameof(value)} invalid");

            Value = value;
        }

        public static GroupSlug From(string value) => new(value);
        public static implicit operator string(GroupSlug group) => group.Value;
        public static implicit operator GroupSlug(string group) => From(group);
    }

    public record Details
    {
        public string Title       { get; init; }
        public string Description { get; init; }

        Details(string title, string description)
        {
            Title = (string.IsNullOrWhiteSpace(title), title?.Length) switch
            {
                (true, _)    => throw new ArgumentNullException(nameof(title)),
                (false, >50) => throw new ArgumentException($"{nameof(title)} has 50 max length"),
                (_, _)       => title!
            };

            Description = (string.IsNullOrWhiteSpace(description), description?.Length) switch
            {
                (true, _)       => throw new ArgumentNullException(nameof(description)),
                (false, > 4096) => throw new ArgumentException($"{nameof(description)} has 4096 max length"),
                (_, _)          => description!
            };
        }

        public static Details From(string title, string description) => new(title, description);
    }
}