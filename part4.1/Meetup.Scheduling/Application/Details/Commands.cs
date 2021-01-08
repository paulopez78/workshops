using System;
using System.ComponentModel.DataAnnotations;

namespace Meetup.Scheduling.Application.Details
{
    public static class Commands
    {
        public static class V1
        {
            public record Create(string Group, [Required] string Title);

            public record UpdateDetails(Guid EventId, [Required] string Title);

            public record Publish(Guid EventId);

            public record Cancel(Guid EventId);
        }
    }
}