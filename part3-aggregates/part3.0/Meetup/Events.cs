using System;

namespace Meetup.Scheduling
{
    public static class Events
    {
        public static class V1
        {
            public record AttendantMovedToWaitingList(Attendant Attendant);

            public record AttendantMovedToGoingList(Attendant Attendant);

            public record Attendant(Guid eventId, Guid userId);
        }
    }
}