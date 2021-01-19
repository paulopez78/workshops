using System;

namespace Meetup.Scheduling.AttendantList
{
    public static class Commands
    {
        public static class V1
        {
            public record Create(Guid Id, Guid MeetupEventId, int Capacity);

            public record Open(Guid Id);

            public record Close (Guid Id);

            public record Attend(Guid Id, Guid UserId);

            public record DontAttend(Guid Id, Guid UserId);

            public record IncreaseCapacity(Guid Id, int Capacity);

            public record ReduceCapacity(Guid Id, int Capacity);
        }
    }
}