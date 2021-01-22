using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Contracts;
using MeetupDetailsEvents = Meetup.Scheduling.Contracts.MeetupDetailsEvents.V1;
using AttendantListEvents = Meetup.Scheduling.Contracts.AttendantListEvents.V1;


namespace Meetup.Scheduling.IntegrationEventsPublisher
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
                new IntegrationEvents.V1.MeetupPublished(context.Message.Id, context.Message.GroupSlug)
            );

        public Task Consume(ConsumeContext<MeetupDetailsEvents.Cancelled> context)
            => context.Publish(
                new IntegrationEvents.V1.MeetupCancelled(context.Message.Id, context.Message.GroupSlug, context.Message.Reason)
            );

        public Task Consume(ConsumeContext<AttendantListEvents.AttendantAddedToWaitingList> context)
            => context.Publish(
                new IntegrationEvents.V1.MeetupAttendantsAddedToWaitingList(context.Message.MeetupEventId, context.Message.UserId)
            );

        public Task Consume(ConsumeContext<AttendantListEvents.AttendantsAddedToWaitingList> context)
            => context.Publish(
                new IntegrationEvents.V1.MeetupAttendantsAddedToWaitingList(context.Message.MeetupEventId, context.Message.Attendants)
            );

        public Task Consume(ConsumeContext<AttendantListEvents.AttendantsRemovedFromWaitingList> context)
            => context.Publish(
                new IntegrationEvents.V1.MeetupAttendantsRemovedFromWaitingList(context.Message.MeetupEventId, context.Message.Attendants)
            );

        public Task Consume(ConsumeContext<AttendantListEvents.AttendantAdded> context)
            => context.Publish(
                new IntegrationEvents.V1.MeetupAttendantAdded(context.Message.MeetupEventId, context.Message.UserId)
            );
    }
}