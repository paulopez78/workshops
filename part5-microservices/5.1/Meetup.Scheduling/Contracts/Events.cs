using System;

namespace Meetup.Scheduling.Contracts
{
    public static class Events
    {
        public static class V1
        {
            public record MeetupAttendantAdded(Guid MeetupEventId, Guid AttendantId);

            public record MeetupAttendantsRemovedFromWaitingList(Guid MeetupEventId, params Guid[] Attendants);

            public record MeetupAttendantsAddedToWaitingList(Guid MeetupEventId, params Guid[] Attendants);

            public record MeetupPublished(Guid MeetupId);

            public record MeetupCancelled(Guid MeetupId, string Reason);
        }
    }
}