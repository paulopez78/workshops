using System;
using System.ComponentModel.DataAnnotations;

namespace Meetup.Scheduling.MeetupDetails
{
    public static class Commands
    {
        public static class V1
        {
            public record Create(Guid EventId, string Group, [Required] string Title, string Description, int Capacity);

            public record UpdateDetails(Guid EventId, [Required] string Title, string Description);

            public record MakeOnline(Guid EventId, [Required] string Url);

            public record MakeOnsite(Guid EventId, [Required] string Address);

            public record Schedule(Guid EventId, [Required] DateTimeOffset StartTime, DateTimeOffset EndTime);

            public record Publish(Guid EventId);

            public record Cancel(Guid EventId, string Reason);
        }
    }
}