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

        public Task<Guid> Handle(object command)
            =>
                command switch
                {
                    CreateAttendantList cmd
                        => Repository.Save(new Domain.AttendantList(Guid.NewGuid(), cmd.meetupEventId, cmd.Capacity)),
                    IncreaseCapacity cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.IncreaseCapacity(cmd.Capacity)
                        ),
                    ReduceCapacity cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.ReduceCapacity(cmd.Capacity)
                        ),
                    AcceptInvitation cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.AcceptInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    DeclineInvitation cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.DeclineInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };

        async Task<Guid> Handle(Guid id, Action<Domain.AttendantList> action)
        {
            var entity = await Repository.Load(id);
            if (entity is null) throw new ApplicationException($"Entity not found {id}");

            action(entity);

            await Repository.Save(entity);

            return id;
        }
    }
}