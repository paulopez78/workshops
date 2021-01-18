using System;
using System.Collections.Immutable;

namespace Meetup.Scheduling.Queries
{
    public record MeetupEvent (Guid Id, string Title, string Group, int Capacity, string Status)
    {
        public ImmutableList<Attendant> Attendants { get; init; } = ImmutableList<Attendant>.Empty;
    }

    public record Attendant(Guid UserId, bool Waiting, DateTimeOffset AddedAt);
}