using System;

namespace Meetup.GroupManagement.Contracts
{
    public static class Events
    {
        public static class V1
        {
            public record MeetupGroupFounded(Guid GroupId, string GroupSlug, string Title, DateTimeOffset FoundedAt);

            public record MeetupGroupMemberJoined (Guid GroupId, string GroupSlug, Guid UserId, DateTimeOffset JoinedAt);
        }
    }
}