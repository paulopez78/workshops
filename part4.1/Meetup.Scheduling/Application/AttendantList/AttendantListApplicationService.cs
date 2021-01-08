using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.Application.AttendantList.Commands.V1;

namespace Meetup.Scheduling.Application.AttendantList
{
    public class AttendantListApplicationService : IApplicationService
    {
        readonly AttendantListRepository                  Repository;
        readonly ILogger<AttendantListApplicationService> Logger;

        public AttendantListApplicationService(AttendantListRepository repository,
            ILogger<AttendantListApplicationService> logger)
        {
            Repository = repository;
            Logger     = logger;
        }

        public Task<CommandResult> Handle(object command)
            =>
                command switch
                {
                    CreateAttendantList cmd
                        => Handle(new Domain.AttendantList(Guid.NewGuid(), cmd.MeetupEventId, cmd.Capacity)),
                    IncreaseCapacity cmd
                        => Handle(
                            cmd.AttendantListId,
                            entity => entity.IncreaseCapacity(cmd.Capacity)
                        ),
                    ReduceCapacity cmd
                        => Handle(
                            cmd.AttendantListId,
                            entity => entity.ReduceCapacity(cmd.Capacity)
                        ),
                    AcceptInvitation cmd
                        => Handle(
                            cmd.AttendantListId,
                            entity => entity.AcceptInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    DeclineInvitation cmd
                        => Handle(
                            cmd.AttendantListId,
                            entity => entity.DeclineInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };

        async Task<CommandResult> Handle(Domain.AttendantList entity)
        {
            var id = await Repository.Save(entity);
            return new(id);
        }

        async Task<CommandResult> Handle(Guid id, Action<Domain.AttendantList> action)
        {
            var entity = await Repository.Load(id);
            if (entity is null) throw new ApplicationException($"Entity not found {id}");

            action(entity);

            await Repository.Save(entity);

            return new(id);
        }
    }
}