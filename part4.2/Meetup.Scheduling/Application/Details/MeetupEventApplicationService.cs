using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.Application.Details.Commands.V1;

namespace Meetup.Scheduling.Application.Details
{
    public class MeetupEventDetailsApplicationService : IApplicationService
    {
        readonly MeetupEventDetailsRepository MeetupEventRepository;

        public MeetupEventDetailsApplicationService(MeetupEventDetailsRepository meetupEventRepository)
            => MeetupEventRepository = meetupEventRepository;

        public Task<CommandResult> Handle(object command)
            =>
                command switch
                {
                    Create cmd
                        => Handle(new MeetupEventDetailsAggregate(Guid.NewGuid(), cmd.Group, cmd.Title)),
                    UpdateDetails cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.UpdateDetails(cmd.Title)
                        ),
                    Publish cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.Publish()
                        ),
                    Cancel cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.Cancel()
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };

        async Task<CommandResult> Handle(MeetupEventDetailsAggregate entity)
        {
            var id = await MeetupEventRepository.Save(entity);
            return new(id);
        }

        async Task<CommandResult> Handle(Guid id, Action<MeetupEventDetailsAggregate> action)
        {
            var entity = await MeetupEventRepository.Load(id);
            if (entity is null) throw new ApplicationException($"Entity not found {id}");

            action(entity);

            await MeetupEventRepository.Save(entity);

            return new(id);
        }
    }
}