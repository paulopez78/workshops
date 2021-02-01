using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.Application.AttendantList.Commands.V1;

namespace Meetup.Scheduling.Application.AttendantList
{
    public class AttendantListApplicationService : ApplicationService<AttendantListAggregate>
    {
        readonly UtcNow GetUtcNow;

        public AttendantListApplicationService(MeetupRepository<AttendantListAggregate> repository, UtcNow getUtcNow) :
            base(repository) => GetUtcNow = getUtcNow;

        public override Task<CommandResult> Handle(Guid aggregateId, object command, CommandContext context)
            =>
                command switch
                {
                    CreateAttendantList cmd
                        => HandleCreate(
                            new AttendantListAggregate(cmd.MeetupEventId, cmd.Capacity), context
                        ),
                    Open _
                        => Handle(
                            aggregateId,
                            entity => entity.Open(),
                            context
                        ),
                    Close _
                        => Handle(
                            aggregateId,
                            entity => entity.Open(),
                            context
                        ),
                    IncreaseCapacity cmd
                        => Handle(
                            aggregateId,
                            entity => entity.IncreaseCapacity(cmd.Capacity),
                            context
                        ),
                    ReduceCapacity cmd
                        => Handle(
                            aggregateId,
                            entity => entity.ReduceCapacity(cmd.Capacity),
                            context
                        ),
                    AcceptInvitation cmd
                        => Handle(
                            aggregateId,
                            entity => entity.AcceptInvitation(cmd.UserId, GetUtcNow()),
                            context
                        ),
                    DeclineInvitation cmd
                        => Handle(
                            aggregateId,
                            entity => entity.DeclineInvitation(cmd.UserId, GetUtcNow()),
                            context
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };
    }
}