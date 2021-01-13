using System;
using static Meetup.Scheduling.Commands.V1;

namespace Meetup.Scheduling
{
    public class MeetupEventApplicationService
    {
        readonly MeetupEventRepository MeetupEventRepository;

        public MeetupEventApplicationService(MeetupEventRepository meetupEventRepository)
            => MeetupEventRepository = meetupEventRepository;

        public Guid Handle(object command)
            =>
                command switch
                {
                    Create cmd
                        => MeetupEventRepository.Save(new MeetupEventEntity(Guid.NewGuid(), cmd.Group, cmd.Title,
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
                            entity => entity.AcceptInvitation(cmd.UserId)
                        ),
                    DeclineInvitation cmd
                        => Execute(
                            cmd.Id,
                            entity => entity.DeclineInvitation(cmd.UserId)
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };

        Guid Execute(Guid id, Action<MeetupEventEntity> action)
        {
            var entity = MeetupEventRepository.Load(id);
            if (entity is null) throw new ApplicationException($"Entity not found {id}");

            action(entity);

            MeetupEventRepository.Save(entity);
            return id;
        }
    }
}