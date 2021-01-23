using System;
using System.Linq;
using Marten.Events.Projections;
using Meetup.Scheduling.Contracts;
using static System.Collections.Immutable.ImmutableList;
using static Meetup.Scheduling.Contracts.MeetupDetailsEvents.V1;
using static Meetup.Scheduling.Contracts.AttendantListEvents.V1;

namespace Meetup.Scheduling.AsyncProjector
{
    public class MeetupEventProjection : ViewProjection<ReadModels.V1.MeetupEvent, Guid>
    {
        public MeetupEventProjection()
        {
            ProjectEvent<Created>((state, created) =>
                {
                    state.Id          = created.Id;
                    state.Title       = created.Title;
                    state.Description = created.Description;
                    state.Capacity    = created.Capacity;
                    state.Group       = created.Group;
                    state.Status      = "Draft";
                }
            );

            ProjectEvent<DetailsUpdated>((state, details) =>
            {
                state.Title       = details.Title;
                state.Description = details.Description;
            });

            ProjectEvent<Scheduled>((state, scheduled) =>
            {
                state.Start = scheduled.Start;
                state.End   = scheduled.End;
            });

            ProjectEvent<MadeOnline>((state, location) =>
            {
                state.Online   = true;
                state.Location = location.Url;
            });

            ProjectEvent<MadeOnsite>((state, location) =>
            {
                state.Online   = false;
                state.Location = location.Address;
            });

            ProjectEvent<Published>((state, _) => { state.Status = "Published"; })/**/;

            ProjectEvent<Cancelled>((state, _) => { state.Status = "Cancelled"; });

            ProjectEvent<AttendantListCreated>(e => e.MeeupEventId, (state, created) =>
            {
                state.AttendantListId     = created.Id;
                state.Attendants          = Create<ReadModels.V1.Attendant>();
                state.AttendantListStatus = "Closed";
            });

            ProjectEvent<Opened>(e => e.MeeupEventId, (state, _) => state.AttendantListStatus   = "Opened");
            ProjectEvent<Closed>(e => e.MeeupEventId, (state, _) => state.AttendantListStatus   = "Closed");
            ProjectEvent<Archived>(e => e.MeeupEventId, (state, _) => state.AttendantListStatus = "Archived");

            ProjectEvent<AttendantAdded>(e => e.MeetupEventId,
                (state, added) =>
                {
                    state.Attendants = state.Attendants.Add(
                        new ReadModels.V1.Attendant {UserId = added.UserId, AddedAt = added.AddedAt}
                    );
                });

            ProjectEvent<AttendantAddedToWaitingList>(e => e.MeetupEventId,
                (state, added) =>
                {
                    state.Attendants = state.Attendants.Add(
                        new ReadModels.V1.Attendant {UserId = added.UserId, Waiting = true, AddedAt = added.AddedAt}
                    );
                });

            ProjectEvent<AttendantRemoved>(e => e.MeetupEventId,
                (state, removed) =>
                {
                    state.Attendants = state.Attendants.RemoveAll(x => x.UserId == removed.UserId);
                });

            ProjectEvent<AttendantsAddedToWaitingList>(e => e.MeetupEventId, (state, added)
                => UpdateWaitingList(state, true, added.Attendants)
            );

            ProjectEvent<AttendantsRemovedFromWaitingList>(e => e.MeetupEventId, (state, removed)
                => UpdateWaitingList(state, false, removed.Attendants)
            );

            static void UpdateWaitingList(ReadModels.V1.MeetupEvent state, bool waiting, params Guid[] userIds)
            {
                foreach (var userId in userIds)
                {
                    var attendant = state.Attendants.FirstOrDefault(x => x.UserId == userId);
                    if (attendant is not null)
                        attendant.Waiting = waiting;
                }
            }
        }
    }
}