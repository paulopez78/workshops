﻿using System;
using System.Collections.Generic;

namespace MeetupEvents.Contracts
{
    public static class ReadModels
    {
        public static class V1
        {
            public record MeetupEvent
            {
                public Guid            Id                  { get; set; }
                public Guid            GroupId             { get; set; }
                public string          Title               { get; set; }
                public string          Description         { get; set; }
                public string          Status              { get; set; }
                public Guid            AttendantListId     { get; set; }
                public int             Capacity            { get; set; }
                public string          AttendantListStatus { get; set; }
                public List<Attendant> Attendants          { get; set; } = new();
            }

            public record Attendant(
                Guid Id,
                Guid UserId,
                DateTime At,
                bool Waiting);
        }
    }
}