using System;

namespace Meetup.GroupManagement.Contracts
{
    public static class Events
    {
        public static class V1
        {
            public record MeetupGroupFounded(Guid Id, string GroupSlug, string Title, DateTimeOffset FoundedAt);

            public record MeetupGroupMemberJoined (Guid Id, string GroupSlug, Guid UserId, DateTimeOffset JoinedAt);

            public record MeetupGroupMemberLeft (Guid Id, string GroupSlug, Guid UserId, DateTimeOffset LeftAt);
        }
    }
}