using System;

namespace Meetup.Scheduling.Contracts
{
    public static class IntegrationEvents
    {
        public static class V1
        {
            public record MeetupAttendantAdded(Guid MeetupEventId, Guid AttendantId);

            public record MeetupAttendantsRemovedFromWaitingList(Guid MeetupEventId, params Guid[] Attendants);

            public record MeetupAttendantsAddedToWaitingList(Guid MeetupEventId, params Guid[] Attendants);

            public record MeetupPublished(Guid MeetupId, string GroupSlug);

            public record MeetupCancelled(Guid MeetupId, string GroupSlug, string Reason);
        }
    }
    
    public static class AttendantListEvents
    {
        public static class V1
        {
            public record AttendantListCreated (Guid Id, Guid MeeupEventId, int Capacity);

            public record Opened(Guid Id, Guid MeeupEventId);

            public record Closed(Guid Id, Guid MeeupEventId);

            public record CapacityReduced(Guid Id, Guid MeeupEventId, int ByNumber);

            public record CapacityIncreased(Guid Id, Guid MeeupEventId, int ByNumber);

            public record AttendantRemoved(Guid Id, Guid MeetupEventId, Guid UserId, DateTimeOffset RemovedAt);

            public record AttendantAdded(Guid Id, Guid MeetupEventId, Guid UserId, DateTimeOffset AddedAt);

            public record AttendantAddedToWaitingList(Guid Id, Guid MeetupEventId, Guid UserId, DateTimeOffset AddedAt);

            public record AttendantsRemovedFromWaitingList(Guid Id, Guid MeetupEventId, DateTimeOffset RemovedAt,
                params Guid[] Attendants);

            public record AttendantsAddedToWaitingList(Guid Id, Guid MeetupEventId, DateTimeOffset AddedAt,
                params Guid[] Attendants);
        }
    }
    
    public static class MeetupDetailsEvents
    {
        public static class V1
        {
            public record Created (Guid Id, string Group, string Title, string Description, int Capacity);

            public record Published(Guid Id, string GroupSlug);

            public record Cancelled(Guid Id, string GroupSlug, string Reason);

            public record Started(Guid Id);

            public record Finished(Guid Id);

            public record DetailsUpdated(Guid Id, string Title, string Description);

            public record Scheduled(Guid Id, DateTimeOffset Start, DateTimeOffset End);

            public record MadeOnline(Guid Id, string Url);

            public record MadeOnsite(Guid Id, string Address);
        }
    }
}