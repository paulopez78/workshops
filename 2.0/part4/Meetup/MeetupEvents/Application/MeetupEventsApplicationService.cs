using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.MeetupEventsCommands.V1;

namespace MeetupEvents.Application
{
    public class MeetupEventsApplicationService : ApplicationService<MeetupEventAggregate>
    {
        readonly IDateTimeProvider DateTimeProvider;

        public MeetupEventsApplicationService(Repository<MeetupEventAggregate> repository, IDateTimeProvider dateTimeProvider) :
            base(repository)
            => DateTimeProvider = dateTimeProvider;

        public override Task<CommandResult> HandleCommand(Guid id, object command)
        {
            return command switch
            {
                CreateMeetupEvent cmd
                    => HandleCreate(
                        id,
                        entity => entity.Create(id, cmd.GroupId, cmd.Title, cmd.Description)
                    ),

                UpdateDetails cmd
                    => Handle(
                        id,
                        entity => entity.UpdateDetails(cmd.Title, cmd.Description)),

                Publish _
                    => Handle(
                        id,
                        entity => entity.Publish()
                    ),

                Cancel cmd
                    => Handle(
                        id,
                        entity => entity.Cancel(cmd.Reason)
                    ),
                _
                    => throw new InvalidOperationException($"Command handler for {command} does not exist")
            };
        }
    }
}