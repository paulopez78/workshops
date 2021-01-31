using System;

namespace MeetupEvents.Contracts
{
    public static class AttendantListEvents
    {
        public static class V1
        {
            public record AttendantListCreated(Guid Id, Guid MeetupEventId, int Capacity);

            public record Opened(Guid Id, DateTimeOffset at);

            public record Closed(Guid Id, DateTimeOffset at);

            public record CapacityIncreased(Guid Id, int byNumber);

            public record CapacityReduced(Guid Id, int byNumber);

            public record AttendantAdded(Guid Id, Guid MemberId, DateTimeOffset at);

            public record AttendantRemoved(Guid Id, Guid MemberId, DateTimeOffset at);
            public record AttendantMovedToWaiting(Guid Id, Guid MemberId, DateTimeOffset at);
        }
    }
}