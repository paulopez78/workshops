using static Meetup.Scheduling.Contracts.MeetupDetailsEvents.V1;
using static Meetup.Scheduling.Contracts.ReadModels.V1;

namespace Meetup.Scheduling.MeetupDetails
{
    public static class MeetupDetailsEventProjection
    {
        public static MeetupDetailsEventReadModel When(MeetupDetailsEventReadModel state, object @event) =>
            @event switch
            {
                Created created
                    => new MeetupDetailsEventReadModel(created.Id, created.Title, created.Description, created.Group,
                        created.Capacity,
                        "Draft"),
                DetailsUpdated details
                    => state with
                    {
                        Title = details.Title,
                        Description = details.Description
                    },
                Scheduled scheduled
                    => state with
                    {
                        Start = scheduled.Start,
                        End = scheduled.End
                    },
                MadeOnline online
                    => state with
                    {
                        Online = true,
                        Location = online.Url
                    },
                MadeOnsite onsite
                    => state with
                    {
                        Online = false,
                        Location = onsite.Address
                    },
                Published _
                    => state with
                    {
                        Status = "Published"
                    },
                Cancelled _
                    => state with
                    {
                        Status = "Cancelled"
                    },
                Started _
                    => state with
                    {
                        Status = "Started"
                    },
                Finished _
                    => state with
                    {
                        Status = "Finished"
                    },
                _ => state
            };
    }
}