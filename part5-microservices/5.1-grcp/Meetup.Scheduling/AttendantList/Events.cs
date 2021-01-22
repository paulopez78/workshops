using System;

namespace Meetup.Scheduling.AttendantList
{
    public static class Events
    {
        public static class V1
        {
            public record AttendantListCreated (Guid Id, Guid MeeupEventId, int Capacity);

            public record Opened(Guid Id, Guid MeeupEventId);

            public record Closed(Guid Id, Guid MeeupEventId);

            public record CapacityReduced(Guid Id, Guid MeeupEventId, int ByNumber);

            public record CapacityIncreased(Guid Id, Guid MeeupEventId, int ByNumber);

            public record AttendantRemoved(Guid Id, Guid MeetupEventId, Guid UserId, DateTimeOffset RemovedAt);

            public record AttendantAdded(Guid Id, Guid MeetupEventId, Guid UserId, DateTimeOffset AddedAt);

            public record AttendantAddedToWaitingList(Guid Id, Guid MeetupEventId, Guid UserId, DateTimeOffset AddedAt);

            public record AttendantsRemovedFromWaitingList(Guid Id, Guid MeetupEventId, DateTimeOffset RemovedAt,
                params Guid[] Attendants);

            public record AttendantsAddedToWaitingList(Guid Id, Guid MeetupEventId, DateTimeOffset AddedAt,
                params Guid[] Attendants);
        }
    }
}