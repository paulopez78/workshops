using System;
using System.Collections.Immutable;
using System.Linq;
using static Meetup.Scheduling.AttendantList.Events.V1;

namespace Meetup.Scheduling.AttendantList
{
    public static class AttendantListProjection
    {
        public static AttendantList When(AttendantList state, object @event) =>
            @event switch
            {
                AttendantListCreated created
                    => new(created.Id, created.MeeupEventId, created.Capacity, "Closed"),
                Opened _
                    => state with
                    {
                        Status = "Opened"
                    },
                Closed _
                    => state with
                    {
                        Status = "Closed"
                    },
                AttendantAdded added
                    => state with
                    {
                        Attendants =
                        state.Attendants.Add(new AttendantList.Attendant(added.UserId, false, added.AddedAt))
                    },
                AttendantRemoved removed
                    => state with
                    {
                        Attendants = state.Attendants.RemoveAll(x => x.UserId == removed.UserId)
                    },
                AttendantWaitingAdded waiting
                    => state with
                    {
                        Attendants =
                        state.Attendants.Add(
                            new AttendantList.Attendant(waiting.UserId, true, waiting.AddedAt))
                    },
                AttendantsAddedToWaitingList addedToWaitingList
                    => UpdateWaitingList(state, true, addedToWaitingList.Attendants),
                AttendantsRemovedFromWaitingList removedFromWaitingList
                    => UpdateWaitingList(state, false, removedFromWaitingList.Attendants),
                _ => state
            };

        static AttendantList UpdateWaitingList(AttendantList state, bool waiting, params Guid[] userIds)
        {
            foreach (var userId in userIds)
            {
                var attendant = state.Attendants.FirstOrDefault(x => x.UserId == userId);
                if (attendant is not null)
                {
                    state = state with
                    {
                        Attendants = state.Attendants.Replace(attendant, attendant with {Waiting = waiting})
                    };
                }
            }

            return state;
        }

        public record AttendantList(Guid Id, Guid MeetupEventId, int Capacity, string Status)
        {
            public ImmutableList<Attendant> Attendants { get; init; } = ImmutableList<Attendant>.Empty;

            public record Attendant(Guid UserId, bool Waiting, DateTimeOffset AddedAt);
        }
    }
}