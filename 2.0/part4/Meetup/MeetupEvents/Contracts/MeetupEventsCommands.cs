using System;

namespace MeetupEvents.Contracts
{
    public static class MeetupEventsCommands
    {
        public static class V1
        {
            public record CreateMeetupEvent(Guid Id, Guid GroupId, string Title, string Description);

            public record UpdateDetails(Guid Id, string Title, string Description);

            public record Publish(Guid Id);

            public record Cancel(Guid Id, string Reason);
        }
    }
}