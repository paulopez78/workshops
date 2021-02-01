using System;
using System.Collections.Generic;

namespace Meetup.Scheduling.Application.Queries
{
    public record MeetupEvent (string Id, string Title, string Group, int Capacity, string Status)
    {
        public List<Attendant> Attendants { get; init; } = new();
    }

    public record Attendant(Guid UserId, string Status, DateTimeOffset ModifiedAt);
            
}