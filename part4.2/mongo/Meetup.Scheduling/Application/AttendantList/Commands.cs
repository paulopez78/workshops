using System;

namespace Meetup.Scheduling.Application.AttendantList
{
    public static class Commands
    {
        public static class V1
        {
            public record CreateAttendantList(string MeetupEventId, int Capacity);

            public record AcceptInvitation(string MeetupEventId, Guid UserId);

            public record DeclineInvitation(string MeetupEventId, Guid UserId);

            public record IncreaseCapacity(string MeetupEventId, int Capacity);

            public record ReduceCapacity(string MeetupEventId, int Capacity);
        }
    }
}