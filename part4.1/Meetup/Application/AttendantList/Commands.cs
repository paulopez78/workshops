using System;

namespace Meetup.Scheduling.Application.AttendantList
{
    public static class Commands
    {
        public static class V1
        {
            public record CreateAttendantList(Guid meetupEventId, int Capacity);

            public record AcceptInvitation(Guid EventId, Guid UserId);

            public record DeclineInvitation(Guid EventId, Guid UserId);

            public record IncreaseCapacity(Guid EventId, int Capacity);

            public record ReduceCapacity(Guid EventId, int Capacity);
        }
    }
}