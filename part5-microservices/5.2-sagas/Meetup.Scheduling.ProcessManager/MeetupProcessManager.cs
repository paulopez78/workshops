using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Contracts;
using static Meetup.GroupManagement.Contracts.Events.V1;
using static Meetup.Scheduling.Contracts.IntegrationEvents.V1;
using static Meetup.Notifications.Contracts.Commands.V1;

namespace Meetup.Scheduling.ProcessManager
{
    public class MeetupProcessManager :
        IConsumer<MeetupCreated>,
        IConsumer<MeetupScheduled>,
        IConsumer<MeetupPublished>,
        IConsumer<MeetupCancelled>,
        IConsumer<MeetupStarted>,
        IConsumer<MeetupFinished>,
        IConsumer<MeetupGroupMemberLeft>,
        IConsumer<MeetupAttendantsAddedToWaitingList>,
        IConsumer<MeetupAttendantsRemovedFromWaitingList>
    {
        public Task Consume(ConsumeContext<MeetupCreated> context)
            => context.Send(
                new AttendantListCommands.V1.CreateAttendantList(
                    Guid.NewGuid(),
                    context.Message.MeetupEventId,
                    context.Message.Capacity
                )
            );

        public Task Consume(ConsumeContext<MeetupScheduled> context)
            => Task.WhenAll(
                context.ScheduleSend(
                    context.Message.Start.DateTime,
                    new MeetupDetailsCommands.V1.Start(context.Message.MeetupId)
                ),
                context.ScheduleSend(
                    context.Message.End.DateTime,
                    new MeetupDetailsCommands.V1.Finish(context.Message.MeetupId)
                )
            );

        public Task Consume(ConsumeContext<MeetupPublished> context)
            => Task.WhenAll(
                context.Send(
                    new AttendantListCommands.V1.Open(context.Message.MeetupId)
                ),
                context.Send(
                    new NotifyMeetupPublished(context.Message.MeetupId, context.Message.GroupSlug)
                )
            );

        public Task Consume(ConsumeContext<MeetupCancelled> context)
            => Task.WhenAll(
                context.Send(
                    new AttendantListCommands.V1.Close(context.Message.MeetupId)
                ),
                context.Send(
                    new NotifyMeetupCancelled(context.Message.MeetupId, context.Message.GroupSlug,
                        context.Message.Reason)
                )
            );

        public Task Consume(ConsumeContext<MeetupStarted> context) =>
            context.Send(
                new AttendantListCommands.V1.Open(context.Message.MeetupId)
            );

        public Task Consume(ConsumeContext<MeetupFinished> context) =>
            context.Send(
                new AttendantListCommands.V1.Archive(context.Message.MeetupId)
            );

        public Task Consume(ConsumeContext<MeetupGroupMemberLeft> context) =>
            context.Send(
                new AttendantListCommands.V1.RemoveAttendantFromMeetups(
                    context.Message.UserId, context.Message.GroupSlug)
            );

        public Task Consume(ConsumeContext<MeetupAttendantsAddedToWaitingList> context)
            => Task.WhenAll(
                context.Message.Attendants.Select(attendant =>
                    context.Send(
                        new NotifyMeetupAttendantWaiting(context.Message.MeetupEventId, attendant)
                    )
                )
            );

        public Task Consume(ConsumeContext<MeetupAttendantsRemovedFromWaitingList> context)
            => Task.WhenAll(
                context.Message.Attendants.Select(attendant =>
                    context.Send(
                        new NotifyMeetupAttendantGoing(context.Message.MeetupEventId, attendant)
                    )
                )
            );
    }
}