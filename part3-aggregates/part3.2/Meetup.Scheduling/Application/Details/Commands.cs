using System;
using System.ComponentModel.DataAnnotations;

namespace Meetup.Scheduling.Application.Details
{
    public static class Commands
    {
        public static class V1
        {
            public record Create(string Group, [Required] string Title, string Description);

            public record UpdateDetails(Guid EventId, [Required] string Title, string Description);

            public record MakeOnline(Guid EventId, [Required] string Url);
            
            public record MakeOnsite(Guid EventId, [Required] string Address);

            public record Schedule(Guid EventId, [Required] DateTimeOffset StartTime, DateTimeOffset EndTime);

            public record Publish(Guid EventId);

            public record Cancel(Guid EventId);
        }
    }
}