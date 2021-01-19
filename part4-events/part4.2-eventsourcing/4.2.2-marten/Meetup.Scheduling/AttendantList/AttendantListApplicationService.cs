using System;
using Meetup.Scheduling.Framework;
using static Meetup.Scheduling.AttendantList.Commands.V1;

namespace Meetup.Scheduling.AttendantList
{
    public static class AttendantListApplicationService
    {
        public static HandleCommand<AttendantListAggregate> Handle(this Handle<AttendantListAggregate> handle,  UtcNow getUtcNow)
            => (id, command, context) =>
                command switch
                {
                    Create cmd
                        => handle(
                            id,
                            aggregate => aggregate.Create(cmd.MeetupEventId, cmd.Capacity),
                            context
                        ),
                    Open _
                        => handle(
                            id,
                            aggregate => aggregate.Open(),
                            context
                        ),
                    Close _
                        => handle(
                            id,
                            aggregate => aggregate.Open(),
                            context
                        ),
                    IncreaseCapacity cmd
                        => handle(
                            id,
                            aggregate => aggregate.IncreaseCapacity(cmd.Capacity, getUtcNow()),
                            context
                        ),
                    ReduceCapacity cmd
                        => handle(
                            id,
                            aggregate => aggregate.ReduceCapacity(cmd.Capacity, getUtcNow()),
                            context
                        ),
                    Attend cmd
                        => handle(
                            id,
                            aggregate => aggregate.Add(cmd.UserId, getUtcNow()),
                            context
                        ),
                    DontAttend cmd
                        => handle(
                            id,
                            aggregate => aggregate.Remove(cmd.UserId, getUtcNow()),
                            context
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };
    }
}