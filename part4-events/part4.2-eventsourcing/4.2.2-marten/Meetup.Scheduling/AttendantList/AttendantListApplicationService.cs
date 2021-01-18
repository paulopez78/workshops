using System;
using System.Threading.Tasks;
using Marten;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.AttendantList.Commands.V1;

namespace Meetup.Scheduling.AttendantList
{
    public class AttendantListApplicationService : ApplicationService<AttendantListAggregate>
    {
        readonly UtcNow GetUtcNow;

        public AttendantListApplicationService(IDocumentStore eventStore, UtcNow getUtcNow) :
            base(eventStore)
        {
            GetUtcNow = getUtcNow;
        }

        public override Task<CommandResult> Handle(Guid aggregateId, object command, CommandContext context)
            =>
                command switch
                {
                    CreateAttendantList cmd
                        => Handle(
                            aggregateId,
                            aggregate => aggregate.Create(cmd.Capacity),
                            context
                        ),
                    Open _
                        => Handle(
                            aggregateId,
                            aggregate => aggregate.Open(),
                            context
                        ),
                    Close _
                        => Handle(
                            aggregateId,
                            aggregate => aggregate.Open(),
                            context
                        ),
                    IncreaseCapacity cmd
                        => Handle(
                            aggregateId,
                            aggregate => aggregate.IncreaseCapacity(cmd.Capacity, GetUtcNow()),
                            context
                        ),
                    ReduceCapacity cmd
                        => Handle(
                            aggregateId,
                            aggregate => aggregate.ReduceCapacity(cmd.Capacity, GetUtcNow()),
                            context
                        ),
                    Attend cmd
                        => Handle(
                            aggregateId,
                            aggregate => aggregate.Add(cmd.UserId, GetUtcNow()),
                            context
                        ),
                    DontAttend cmd
                        => Handle(
                            aggregateId,
                            aggregate => aggregate.Remove(cmd.UserId, GetUtcNow()),
                            context
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };
    }
}