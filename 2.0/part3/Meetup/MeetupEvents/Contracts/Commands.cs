﻿using System;

namespace MeetupEvents.Contracts
{
    public static class Commands
    {
        public static class V1
        {
            public record Create(Guid Id, Guid GroupId, string Title, string Description, int Capacity);

            public record UpdateDetails(Guid Id, string Title, string Description);

            public record Publish(Guid Id);

            public record Cancel(Guid Id, string Reason);

            public record Attend(Guid Id, Guid MemberId);

            public record CancelAttendance(Guid Id, Guid MemberId);
        }
    }
}