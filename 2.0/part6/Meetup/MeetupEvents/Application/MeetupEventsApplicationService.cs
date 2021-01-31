using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.MeetupCommands.V1;

namespace MeetupEvents.Application
{
    public class MeetupEventsApplicationService : ApplicationService<MeetupEventAggregate>
    {
        readonly IDateTimeProvider DateTimeProvider;

        public MeetupEventsApplicationService(Repository<MeetupEventAggregate> repository,
            IDateTimeProvider dateTimeProvider) :
            base(repository)
            => DateTimeProvider = dateTimeProvider;

        public override Task<CommandResult> HandleCommand(Guid id, object command)
        {
            return command switch
            {
                CreateMeetupEvent cmd
                    => HandleCreate(
                        id,
                        entity => entity.Create(id, cmd.GroupId, Details.From(cmd.Title, cmd.Description))
                    ),

                UpdateDetails cmd
                    => Handle(
                        id,
                        entity => entity.UpdateDetails(Details.From(cmd.Title, cmd.Description))
                    ),

                MakeOnline cmd
                    => Handle(
                        id,
                        entity => entity.MakeOnline(cmd.Url)
                    ),

                MakeOnsite cmd
                    => Handle(
                        id,
                        entity => entity.MakeOnsite(cmd.Address)
                    ),

                Schedule cmd
                    => Handle(
                        id,
                        entity => entity.Schedule(
                            ScheduleTime.From(DateTimeProvider.GetUtcNow, cmd.Start, cmd.End)
                        )
                    ),

                Publish _
                    => Handle(
                        id,
                        entity => entity.Publish(DateTimeProvider.GetUtcNow())
                    ),
                
                Cancel cmd
                    => Handle(
                        id,
                        entity => entity.Cancel(cmd.Reason, DateTimeProvider.GetUtcNow())
                    ),
                _
                    => throw new InvalidOperationException($"Command handler for {command} does not exist")
            };
        }
    }
}