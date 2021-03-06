using System;
using Meetup.Scheduling.Framework;
using static Meetup.Scheduling.Contracts.MeetupDetailsCommands.V1;

namespace Meetup.Scheduling.MeetupDetails
{
    public static class MeetupDetailsApplicationService
    {
        public static HandleCommand<MeetupDetailsAggregate> Handle(
            this Handle<MeetupDetailsAggregate> handle, UtcNow getUtcNow)
            => (aggregateId, command, context) => command switch
            {
                CreateMeetup cmd
                    => handle(
                        aggregateId,
                        entity => entity.Create(
                            GroupSlug.From(cmd.Group),
                            Details.From(cmd.Title, cmd.Description),
                            cmd.Capacity),
                        context
                    ),
                UpdateDetails cmd
                    => handle(
                        aggregateId,
                        entity => entity.UpdateDetails(Details.From(cmd.Title, cmd.Description)),
                        context
                    ),
                MakeOnline cmd
                    => handle(
                        aggregateId,
                        entity => entity.MakeOnlineEvent(new Uri(cmd.Url)),
                        context
                    ),
                MakeOnsite cmd
                    => handle(
                        aggregateId,
                        entity => entity.MakeOnSiteEvent(Address.From(cmd.Address)),
                        context
                    ),
                Schedule cmd
                    => handle(
                        aggregateId,
                        entity => entity.Schedule(ScheduleDateTime.From(getUtcNow(), cmd.StartTime, cmd.EndTime)),
                        context
                    ),
                Publish _
                    => handle(
                        aggregateId,
                        meetup => meetup.Publish(),
                        context
                    ),
                Cancel cmd
                    => handle(
                        aggregateId,
                        meetup => meetup.Cancel(cmd.Reason),
                        context
                    ),
                Start _
                    => handle(
                        aggregateId,
                        meetup => meetup.Start(),
                        context
                    ),
                Finish _
                    => handle(
                        aggregateId,
                        meetup => meetup.Finish(),
                        context
                    ),
                _
                    => throw new ApplicationException("command handler not found")
            };
    }
}