using System;
using System.Collections.Immutable;
using System.Linq;
using MeetupDetailsEvents = Meetup.Scheduling.MeetupDetails.Events.V1;
using AttendantListEvents = Meetup.Scheduling.AttendantList.Events.V1;

namespace Meetup.Scheduling.Queries
{
    public class MeetupEventProjection
    {
        public static MeetupEvent When(MeetupEvent state, object @event) =>
            @event switch
            {
                MeetupDetailsEvents.Created created
                    => new MeetupEvent(created.Id, created.Title, created.Group, created.Capacity, "Draft"),
                MeetupDetailsEvents.Published _
                    => state with {Status = "Published"},
                MeetupDetailsEvents.Cancelled _
                    => state with {Status = "Cancelled"},
                AttendantList.Events.V1.Created _
                    => state with {Attendants = ImmutableList<Attendant>.Empty},
                AttendantList.Events.V1.AttendantAdded added
                    => state with{ Attendants = state.Attendants.Add(new Attendant(added.UserId, false, added.AddedAt))},
                AttendantList.Events.V1.AttendantRemoved removed 
                    => state with{ Attendants = state.Attendants.RemoveAll(x => x.UserId == removed.UserId)},
                AttendantList.Events.V1.AttendantWaitingAdded waiting
                    => state with{ Attendants = state.Attendants.Add(new Attendant(waiting.UserId, true, waiting.AddedAt))},
                AttendantList.Events.V1.AttendantsAddedToWaitingList addedToWaitingList 
                    => UpdateWaitingList(state, true, addedToWaitingList.Attendants),
                AttendantList.Events.V1.AttendantsRemovedFromWaitingList removedFromWaitingList 
                    => UpdateWaitingList(state, false, removedFromWaitingList.Attendants),
                _ => state
            };

        static MeetupEvent UpdateWaitingList(MeetupEvent state, bool waiting, params Guid[] userIds)
        {
            foreach (var userId in userIds)
            {
                var attendant = state.Attendants.FirstOrDefault(x => x.UserId == userId);
                if (attendant is not null)
                {
                    state = state with {Attendants = state.Attendants.Replace(attendant, attendant with {Waiting = waiting})};
                }
            }

            return state;
        }
    }
}