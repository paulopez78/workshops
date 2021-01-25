using System;
using System.Linq;
using static Meetup.Scheduling.Contracts.AttendantListEvents.V1;
using static Meetup.Scheduling.Contracts.ReadModels.V1;

namespace Meetup.Scheduling.AttendantList
{
    public static class AttendantListProjection
    {
        public static AttendantListReadModel When(AttendantListReadModel state, object @event) =>
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
                        state.Attendants.Add(
                            new Attendant {UserId = added.UserId, Waiting = false, AddedAt = added.AddedAt}
                        )
                    },
                AttendantRemoved removed
                    => state with
                    {
                        Attendants = state.Attendants.RemoveAll(x => x.UserId == removed.UserId)
                    },
                AttendantAddedToWaitingList waiting
                    => state with
                    {
                        Attendants =
                        state.Attendants.Add(
                            new Attendant {UserId = waiting.UserId, Waiting = true, AddedAt = waiting.AddedAt}
                        )
                    },
                AttendantsAddedToWaitingList addedToWaitingList
                    => UpdateWaitingList(state, true, addedToWaitingList.Attendants),
                AttendantsRemovedFromWaitingList removedFromWaitingList
                    => UpdateWaitingList(state, false, removedFromWaitingList.Attendants),
                _ => state
            };

        static AttendantListReadModel UpdateWaitingList(AttendantListReadModel state, bool waiting,
            params Guid[] userIds)
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
    }
}