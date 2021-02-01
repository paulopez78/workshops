using System;

namespace Meetup.Scheduling.Application.AttendantList
{
    public static class Commands
    {
        public static class V1
        {
            public record CreateAttendantList(Guid MeetupEventId, int Capacity);

            public record AcceptInvitation(Guid AttendantListId, Guid UserId);

            public record DeclineInvitation(Guid AttendantListId, Guid UserId);

            public record IncreaseCapacity(Guid AttendantListId, int Capacity);

            public record ReduceCapacity(Guid AttendantListId, int Capacity);
        }
    }
}