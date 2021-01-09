using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.Application.AttendantList.Commands.V1;

namespace Meetup.Scheduling.Application.AttendantList
{
    public class AttendantListApplicationService : IApplicationService
    {
        readonly AttendantListRepository Repository;

        public AttendantListApplicationService(AttendantListRepository repository) => Repository = repository;

        public Task<CommandResult> Handle(object command)
            =>
                command switch
                {
                    CreateAttendantList cmd
                        => Handle(new AttendantListAggregate(cmd.MeetupEventId, cmd.Capacity)),
                    IncreaseCapacity cmd
                        => Handle(
                            cmd.MeetupEventId,
                            entity => entity.IncreaseCapacity(cmd.Capacity)
                        ),
                    ReduceCapacity cmd
                        => Handle(
                            cmd.MeetupEventId,
                            entity => entity.ReduceCapacity(cmd.Capacity)
                        ),
                    AcceptInvitation cmd
                        => Handle(
                            cmd.MeetupEventId,
                            entity => entity.AcceptInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    DeclineInvitation cmd
                        => Handle(
                            cmd.MeetupEventId,
                            entity => entity.DeclineInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };

        async Task<CommandResult> Handle(AttendantListAggregate aggregate)
        {
            var id = await Repository.Save(aggregate);
            return new(id);
        }

        async Task<CommandResult> Handle(Guid id, Action<AttendantListAggregate> action)
        {
            var aggregate = await Repository.Load(id);
            if (aggregate is null) throw new ApplicationException($"Aggregate not found {id}");

            action(aggregate);

            await Repository.Save(aggregate);

            return new(id);
        }
    }
}