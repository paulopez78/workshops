using System;
using System.Text.RegularExpressions;

namespace Meetup.Scheduling.Domain
{
    public record Location
    {
        public bool   IsOnline { get; }
        public Uri    Url      { get; }
        public string Address  { get; }
    }

    public record DateTimeRange
    {
        public DateTimeOffset Start { get; }
        public DateTimeOffset End   { get; }
    }

    public record GroupSlug
    {
        public string Value { get; }

        GroupSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentNullException(nameof(slug));

            Regex regex = new(@"^[a-z\d](?:[a-z\d_-]*[a-z\d])?$");
            if (!regex.IsMatch(slug))
                throw new ArgumentException($"{nameof(slug)} invalid");

            Value = slug;
        }

        GroupSlug From(string value) => new(value);
    }

    public record Description
    {
        public string Value { get; }

        Description(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));

            Value = description;
        }

        Description From(string value) => new(value);
    }

    public record Title
    {
        public string Value { get; }

        Title(string title)
        {
            Value = (string.IsNullOrWhiteSpace(title), title?.Length) switch
            {
                (true, _)    => throw new ArgumentNullException(nameof(title)),
                (false, >50) => throw new ArgumentException($"{nameof(title)} has 50 max length"),
                (_, _)       => title!
            };
        }

        Title From(string value) => new(value);
    }
}