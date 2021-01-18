using System;

namespace Meetup.Scheduling.AttendantList
{
    public static class Events
    {
        public static class V1
        {
            public record Created (Guid Id, int Capacity);

            public record Opened(Guid Id);

            public record Closed(Guid Id);

            public record CapacityReduced(Guid Id, int ByNumber);

            public record CapacityIncreased(Guid Id, int ByNumber);

            public record AttendantRemoved(Guid EventId, Guid UserId, DateTimeOffset RemovedAt);

            public record AttendantAdded(Guid EventId, Guid UserId, DateTimeOffset AddedAt);

            public record AttendantWaitingAdded(Guid EventId, Guid UserId, DateTimeOffset AddedAt);

            public record AttendantsRemovedFromWaitingList(Guid EventId, DateTimeOffset RemovedAt, params Guid[] Attendants);

            public record AttendantsAddedToWaitingList(Guid EventId, DateTimeOffset AddedAt, params Guid[] Attendants);
        }
    }
}