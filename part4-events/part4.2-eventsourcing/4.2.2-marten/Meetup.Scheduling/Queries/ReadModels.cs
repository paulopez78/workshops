using System;
using System.Collections.Generic;

namespace Meetup.Scheduling.Queries
{
    public record MeetupEvent (Guid Id, string Title, string Description, string Group, int Capacity, string Status,
        DateTimeOffset Start, DateTimeOffset End, string Location, bool Online, Guid? AttendantListId, List<Attendant>? Attendants)
    {
    }

    public record Attendant(Guid UserId, bool Waiting, DateTimeOffset AddedAt);
}