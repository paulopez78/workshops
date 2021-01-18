using System;
using System.Threading.Tasks;
using Marten;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.MeetupDetails.Commands.V1;

namespace Meetup.Scheduling.MeetupDetails
{
    public class MeetupEventDetailsApplicationService : ApplicationService<MeetupEventDetailsAggregate>
    {
        readonly IDateTimeProvider DateTimeProvider;

        public MeetupEventDetailsApplicationService(IDocumentStore eventStore, IDateTimeProvider dateTimeProvider) :
            base(eventStore) =>
            DateTimeProvider = dateTimeProvider;

        public override Task<CommandResult> Handle(Guid aggregateId, object command, CommandContext context)
        {
            return command switch
            {
                Create cmd
                    => Handle(
                        aggregateId,
                        entity => entity.Create(
                            GroupSlug.From(cmd.Group),
                            Details.From(cmd.Title, cmd.Description),
                            cmd.Capacity),
                        context
                    ),
                UpdateDetails cmd
                    => Handle(
                        aggregateId,
                        entity => entity.UpdateDetails(Details.From(cmd.Title, cmd.Description)),
                        context
                    ),
                MakeOnline cmd
                    => Handle(
                        aggregateId,
                        entity => entity.MakeOnlineEvent(new Uri(cmd.Url)),
                        context
                    ),
                MakeOnsite cmd
                    => Handle(
                        aggregateId,
                        entity => entity.MakeOnSiteEvent(Address.From(cmd.Address)),
                        context
                    ),
                Schedule cmd
                    => Handle(
                        aggregateId,
                        entity => entity.Schedule(ScheduleDateTime.From(DateTimeProvider.UtcNow(), cmd.StartTime,
                            cmd.EndTime)),
                        context
                    ),
                Publish _
                    => Handle(
                        aggregateId,
                        meetup => meetup.Publish(),
                        context
                    ),
                Cancel cmd
                    => Handle(
                        aggregateId,
                        meetup => meetup.Cancel(cmd.Reason),
                        context
                    ),
                _
                    => throw new ApplicationException("command handler not found")
            };
        }
    }
}