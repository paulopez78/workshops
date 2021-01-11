using System;
using System.Collections.Generic;

namespace Meetup.Scheduling.Application.Queries.Data
{
    public record MeetupEventDocument(string Id, Group Group, Details Details, string Status);

    public record Group(string Value);

    public record Details(string Title, string Description);

    public record AttendantListDocument(string Id, AttendantList AttendantList);

    public record AttendantList(PositiveNumber Capacity, List<Attendant> Attendants);

    public record Attendant(Guid UserId, string Status, DateTimeOffset ModifiedAt);

    public record PositiveNumber(int Value);
}