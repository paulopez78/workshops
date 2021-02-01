using System;

namespace Meetup.Scheduling.Domain
{
    public static class Events
    {
        public static class V1
        {
            public static class MeetupEvent
            {
                public record Created (Guid Id,string Group, string Title, string Description);
                public record Published(Guid Id);
                public record Cancelled(Guid Id, string Reason);
                public record Started(Guid Id);
                public record Finished(Guid Id);
                
                public record DetailsUpdated(Guid Id, string Title, string Description);
                public record Scheduled(Guid Id, DateTimeOffset Start, DateTimeOffset End);
                public record MadeOnline(Guid Id, string Url);
                public record MadeOnsite(Guid Id, string Address);
            }

            public static class AttendantList
            {
                public record Created (Guid Id, int Capacity);

                public record Opened(Guid Id);

                public record Closed(Guid Id);

                public record CapacityReduced(Guid Id, int ByNumber);

                public record CapacityIncreased(Guid Id, int ByNumber);

                public record AttendantMovedToNotGoingList(Attendant Attendant);

                public record AttendantMovedToWaitingList(Attendant Attendant);

                public record AttendantMovedToGoingList(Attendant Attendant);

                public record Attendant(Guid EventId, Guid UserId, DateTimeOffset ModifiedAt);
            }
        }
    }
}