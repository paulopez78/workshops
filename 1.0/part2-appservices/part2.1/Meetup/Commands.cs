using System;

namespace Meetup.Scheduling
{
    public static class Commands
    {
        public static class V1
        {
            public record Create(string Group, string Title, int Capacity);

            public record Publish(Guid Id);

            public record Cancel(Guid Id);

            public record AcceptInvitation(Guid Id, Guid UserId);

            public record DeclineInvitation(Guid Id, Guid UserId);

            public record IncreaseCapacity(Guid Id, int Capacity);

            public record ReduceCapacity(Guid Id, int Capacity);
        }
    }
}