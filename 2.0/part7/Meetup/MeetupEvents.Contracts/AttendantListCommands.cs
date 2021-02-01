using System;

namespace MeetupEvents.Contracts
{
    public static class AttendantListCommands
    {
        public static class V1
        {
            public record CreateAttendantList(Guid Id, Guid MeetupEventId, int Capacity);

            public record Open(Guid MeetupEventId);

            public record Close(Guid MeetupEventId);

            public record IncreaseCapacity(Guid MeetupEventId, int byNumber);

            public record ReduceCapacity(Guid MeetupEventId, int byNumber);

            public record Attend(Guid MeetupEventId, Guid MemberId);

            public record CancelAttendance(Guid MeetupEventId, Guid MemberId);
        }
    }
}