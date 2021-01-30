using System;
using System.ComponentModel.DataAnnotations;

namespace MeetupEvents.Contracts
{
    public static class MeetupEventsCommands
    {
        public static class V1
        {
            public record CreateMeetupEvent(Guid Id, Guid GroupId, [Required] string Title,
                [Required] string Description);

            public record UpdateDetails(Guid Id, [Required] string Title, [Required] string Description);

            public record MakeOnline(Guid Id, Uri Url);

            public record MakeOnsite(Guid Id, string Address);

            public record Schedule(Guid Id, DateTimeOffset Start, DateTimeOffset End);

            public record Publish(Guid Id);

            public record Cancel(Guid Id, string Reason);
        }
    }
}