using System;
using System.Threading.Tasks;
using static Meetup.Scheduling.Commands.V1;

namespace Meetup.Scheduling
{
    public class MeetupEventApplicationService
    {
        readonly IRepository MeetupEventRepository;

        public MeetupEventApplicationService(IRepository meetupEventRepository)
            => MeetupEventRepository = meetupEventRepository;

        public Task<Guid> Handle(object command)
            =>
                command switch
                {
                    Create cmd
                        => MeetupEventRepository.Save(new Domain.MeetupEvent(Guid.NewGuid(), cmd.Group, cmd.Title,
                            cmd.Capacity)),
                    Publish cmd
                        => Execute(
                            cmd.Id,
                            entity => entity.Publish()
                        ),
                    Cancel cmd
                        => Execute(
                            cmd.Id,
                            entity => entity.Cancel()
                        ),
                    IncreaseCapacity cmd
                        => Execute(
                            cmd.Id,
                            entity => entity.IncreaseCapacity(cmd.Capacity)
                        ),
                    ReduceCapacity cmd
                        => Execute(
                            cmd.Id,
                            entity => entity.ReduceCapacity(cmd.Capacity)
                        ),
                    AcceptInvitation cmd
                        => Execute(
                            cmd.Id,
                            entity => entity.AcceptInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    DeclineInvitation cmd
                        => Execute(
                            cmd.Id,
                            entity => entity.DeclineInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };

        async Task<Guid> Execute(Guid id, Action<Domain.MeetupEvent> action)
        {
            var entity = await MeetupEventRepository.Load(id);
            if (entity is null) throw new ApplicationException($"Entity not found {id}");

            action(entity);

            await MeetupEventRepository.Save(entity);
            return id;
        }
    }
}