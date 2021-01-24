using System;
using Marten;
using Meetup.Scheduling.Framework;
using static Meetup.Scheduling.Contracts.AttendantListCommands.V1;

namespace Meetup.Scheduling.AttendantList
{
    public static class AttendantListApplicationService
    {
        public static HandleCommand<AttendantListAggregate> Handle(this Handle<AttendantListAggregate> handle,
            UtcNow getUtcNow)
            => (id, command, context) =>
                command switch
                {
                    CreateAttendantList cmd
                        => handle(
                            id,
                            aggregate => aggregate.Create(cmd.MeetupEventId, cmd.Capacity),
                            context
                        ),
                    Open _
                        => handle(
                            id,
                            aggregate => aggregate.Open(getUtcNow()),
                            context
                        ),
                    Close _
                        => handle(
                            id,
                            aggregate => aggregate.Close(getUtcNow()),
                            context
                        ),
                    Archive _
                        => handle(
                            id,
                            aggregate => aggregate.Archive(getUtcNow()),
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

        public static HandleCommand<AttendantListAggregate> MappingId(this HandleCommand<AttendantListAggregate> handle,
            IDocumentStore store)
            => async (id, command, context) =>
                command switch
                {
                    CreateAttendantList _ => await handle(id, command, context),
                    _                     => await handle(await store.GetAttendantListId(id), command, context),
                };
    }
}