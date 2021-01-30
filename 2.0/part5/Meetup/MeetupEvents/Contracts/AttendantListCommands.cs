using System;

namespace MeetupEvents.Contracts
{
    public static class AttendantListCommands
    {
        public static class V1
        {
            public record CreateAttendantList(Guid Id, Guid MeetupEventId, int Capacity);

            public record Open(Guid Id);

            public record Close(Guid Id);

            public record Attend(Guid Id, Guid MemberId);

            public record CancelAttendance(Guid Id, Guid MemberId);
        }
    }
}