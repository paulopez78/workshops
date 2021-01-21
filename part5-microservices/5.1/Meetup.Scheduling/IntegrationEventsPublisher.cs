using System.Threading.Tasks;
using MassTransit;
using MeetupDetailsEvents = Meetup.Scheduling.MeetupDetails.Events.V1;
using AttendantListEvents = Meetup.Scheduling.AttendantList.Events.V1;
using static Meetup.Scheduling.Contracts.Events.V1;

namespace Meetup.Scheduling
{
    public class IntegrationEventsPublisher :
        IConsumer<MeetupDetailsEvents.Published>,
        IConsumer<MeetupDetailsEvents.Cancelled>,
        IConsumer<AttendantListEvents.AttendantAdded>,
        IConsumer<AttendantListEvents.AttendantAddedToWaitingList>,
        IConsumer<AttendantListEvents.AttendantsAddedToWaitingList>,
        IConsumer<AttendantListEvents.AttendantsRemovedFromWaitingList>
    {
        public Task Consume(ConsumeContext<MeetupDetailsEvents.Published> context)
            => context.Publish(
                new MeetupPublished(context.Message.Id)
            );

        public Task Consume(ConsumeContext<MeetupDetailsEvents.Cancelled> context)
            => context.Publish(
                new MeetupCancelled(context.Message.Id, context.Message.Reason)
            );

        public Task Consume(ConsumeContext<AttendantListEvents.AttendantAddedToWaitingList> context)
            => context.Publish(
                new MeetupAttendantsAddedToWaitingList(context.Message.MeetupEventId, context.Message.UserId)
            );

        public Task Consume(ConsumeContext<AttendantListEvents.AttendantsAddedToWaitingList> context)
            => context.Publish(
                new MeetupAttendantsAddedToWaitingList(context.Message.MeetupEventId, context.Message.Attendants)
            );

        public Task Consume(ConsumeContext<AttendantListEvents.AttendantsRemovedFromWaitingList> context)
            => context.Publish(
                new MeetupAttendantsRemovedFromWaitingList(context.Message.MeetupEventId, context.Message.Attendants)
            );

        public Task Consume(ConsumeContext<AttendantListEvents.AttendantAdded> context)
            => context.Publish(
                new MeetupAttendantAdded(context.Message.MeetupEventId, context.Message.UserId)
            );
    }
}