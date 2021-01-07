using System;
using System.ComponentModel.DataAnnotations;

namespace Meetup.Scheduling
{
    public static class Commands
    {
        public static class V1
        {
            public record Create(string Group, [Required] string Title, int Capacity);

            public record UpdateDetails(Guid EventId, [Required] string Title);

            public record Publish(Guid EventId);

            public record Cancel(Guid EventId);

            public record AcceptInvitation(Guid EventId, Guid UserId);

            public record DeclineInvitation(Guid EventId, Guid UserId);

            public record IncreaseCapacity(Guid EventId, int Capacity);

            public record ReduceCapacity(Guid EventId, int Capacity);
        }
    }
}