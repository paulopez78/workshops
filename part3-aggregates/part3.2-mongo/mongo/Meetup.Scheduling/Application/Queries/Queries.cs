using System;

namespace Meetup.Scheduling.Application.Queries
{
    public static class V1
    {
        public record GetById(string EventId);

        public record GetByGroup(string Group);
    }
}