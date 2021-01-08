using System;
using System.Collections.Generic;

namespace Meetup.Scheduling.Application.Queries
{
    public record MeetupEvent (Guid Id, string Title, string Group, int Capacity, string Status)
    {
        public List<Attendant> Attendants { get; init; } = new();
    }

    public record Attendant(Guid Id, Guid UserId, string Status);
}