using System;

namespace Meetup.Scheduling.Application.AttendantList
{
    public static class Commands
    {
        public static class V1
        {
            public record CreateAttendantList(Guid MeetupEventId, int Capacity);

            public record AcceptInvitation(Guid MeetupEventId, Guid UserId);

            public record DeclineInvitation(Guid MeetupEventId, Guid UserId);

            public record IncreaseCapacity(Guid MeetupEventId, int Capacity);

            public record ReduceCapacity(Guid MeetupEventId, int Capacity);
        }
    }
}