using System;

namespace Meetup.Scheduling.MeetupDetails
{
    public static class Events
    {
        public static class V1
        {
            public record Created (Guid Id, string Group, string Title, string Description, int Capacity);

            public record Published(Guid Id);

            public record Cancelled(Guid Id, string Reason);

            public record Started(Guid Id);

            public record Finished(Guid Id);

            public record DetailsUpdated(Guid Id, string Title, string Description);

            public record Scheduled(Guid Id, DateTimeOffset Start, DateTimeOffset End);

            public record MadeOnline(Guid Id, string Url);

            public record MadeOnsite(Guid Id, string Address);
        }
    }
}