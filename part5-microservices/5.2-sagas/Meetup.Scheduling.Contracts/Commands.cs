using System;
using System.ComponentModel.DataAnnotations;

namespace Meetup.Scheduling.Contracts
{
    public static class MeetupDetailsCommands
    {
        public static class V1
        {
            public record CreateMeetup(Guid EventId, string Group, [Required] string Title, string Description, int Capacity);

            public record UpdateDetails(Guid EventId, [Required] string Title, string Description);

            public record MakeOnline(Guid EventId, [Required] string Url);

            public record MakeOnsite(Guid EventId, [Required] string Address);

            public record Schedule(Guid EventId, [Required] DateTimeOffset StartTime, DateTimeOffset EndTime);

            public record Publish(Guid EventId);

            public record Cancel(Guid EventId, string Reason);
        }
    }
    
    public static class AttendantListCommands
    {
        public static class V1
        {
            public record CreateAttendantList(Guid Id, Guid MeetupEventId, int Capacity);

            public record Open(Guid Id);

            public record Close (Guid Id);

            public record Attend(Guid Id, Guid UserId);

            public record DontAttend(Guid Id, Guid UserId);

            public record IncreaseCapacity(Guid Id, int Capacity);

            public record ReduceCapacity(Guid Id, int Capacity);
        }
    }
}