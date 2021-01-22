using System;
using Meetup.Scheduling.Framework;
using static Meetup.Scheduling.Contracts.MeetupDetailsCommands.V1;

namespace Meetup.Scheduling.MeetupDetails
{
    public static class MeetupEventDetailsApplicationService
    {
        public static HandleCommand<MeetupEventDetailsAggregate> Handle(
            this Handle<MeetupEventDetailsAggregate> handle, UtcNow getUtcNow)
            => (aggregateId, command, context) =>
            {
                return command switch
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
                    _
                        => throw new ApplicationException("command handler not found")
                };
            };
    }
}