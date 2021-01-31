using System;
using System.Threading.Tasks;
using MassTransit;
using MeetupEvents.Queries;
using static MeetupEvents.Contracts.MeetupEvents.V1;
using static MeetupEvents.Contracts.AttendantListEvents.V1;
using static MeetupEvents.Contracts.IntegrationEvents;

namespace MeetupEvents.Application
{
    public class IntegrationEventsDispatcher :
        IConsumer<Published>,
        IConsumer<Canceled>,
        IConsumer<AttendantAdded>,
        IConsumer<AttendantMovedToWaiting>
    {
        private readonly MeetupEventQueries Queries;
        private readonly GetMeetupEventId   GetMeetupId;

        public IntegrationEventsDispatcher(MeetupEventQueries queries, GetMeetupEventId getMeetupId)
        {
            Queries     = queries;
            GetMeetupId = getMeetupId;
        }

        public async Task Consume(ConsumeContext<Published> context)
        {
            await context.Publish(new V1.MeetupEventPublished(context.Message.Id, context.Message.At));

            var meetup = await Queries.Handle(new Contracts.Queries.V1.Get(context.Message.Id));
            if (meetup is null)
                throw new ArgumentException(nameof(context.Message.Id));

            await context.Publish(new V2.MeetupEventPublished(context.Message.Id, meetup.Title, meetup.Description));
        }

        public Task Consume(ConsumeContext<Canceled> context) =>
            context.Publish(new V1.MeetupEventCanceled(context.Message.Id, context.Message.Reason,
                context.Message.At));

        public async Task Consume(ConsumeContext<AttendantAdded> context)
        {
            var meetupId = await GetMeetupId(context.Message.Id);
            if (meetupId is null)
                throw new ArgumentException(nameof(context.Message.Id));

            await context.Publish(new V1.AttendantAdded(meetupId.Value, context.Message.MemberId, context.Message.At));
        }

        public async Task Consume(ConsumeContext<AttendantMovedToWaiting> context)
        {
            var meetupId = await GetMeetupId(context.Message.Id);
            if (meetupId is null)
                throw new ArgumentException(nameof(context.Message.Id));

            await context.Publish(new V1.AttendantMovedToWaiting(meetupId.Value, context.Message.MemberId,
                context.Message.At));
        }
    }
}