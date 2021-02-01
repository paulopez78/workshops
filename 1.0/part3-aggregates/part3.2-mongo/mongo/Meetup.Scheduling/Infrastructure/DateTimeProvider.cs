using System;

namespace Meetup.Scheduling.Infrastructure
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow() => DateTimeOffset.UtcNow;
    }

    public class DateTimeProvider : IDateTimeProvider
    {
    }

    public delegate DateTimeOffset UtcNow();
}