using System;

namespace Meetup.Scheduling.AttendantList
{
    public static class Commands
    {
        public static class V1
        {
            public record CreateAttendantList(Guid MeetupEventId, int Capacity);

            public record Open(Guid MeetupEventId);

            public record Close (Guid MeetupEventId);

            public record Attend(Guid MeetupEventId, Guid UserId);

            public record DontAttend(Guid MeetupEventId, Guid UserId);

            public record IncreaseCapacity(Guid MeetupEventId, int Capacity);

            public record ReduceCapacity(Guid MeetupEventId, int Capacity);
        }
    }
}