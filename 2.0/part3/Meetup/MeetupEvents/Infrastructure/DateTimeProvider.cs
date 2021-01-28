using System;

namespace MeetupEvents.Infrastructure
{
    public interface IDateTimeProvider
    {
        DateTimeOffset GetUtcNow();
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
    }
}