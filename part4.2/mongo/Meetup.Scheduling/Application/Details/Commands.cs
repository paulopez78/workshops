using System;
using System.ComponentModel.DataAnnotations;

namespace Meetup.Scheduling.Application.Details
{
    public static class Commands
    {
        public static class V1
        {
            public record Create(string Group, [Required] string Title, string Description);

            public record UpdateDetails(string EventId, [Required] string Title, string Description);

            public record MakeOnline(string EventId, [Required] string Url);
            
            public record MakeOnsite(string EventId, [Required] string Address);

            public record Schedule(string EventId, [Required] DateTimeOffset StartTime, DateTimeOffset EndTime);

            public record Publish(string EventId);

            public record Cancel(string EventId);
        }
    }
}