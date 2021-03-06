﻿using System;
using System.Threading.Tasks;
using MassTransit;
using static MeetupEvents.Contracts.MeetupEvents.V1;
using static MeetupEvents.Contracts.AttendantListEvents.V1;
using static MeetupEvents.Contracts.IntegrationEvents;

namespace MeetupEvents.EventsDispatcher
{
    public class IntegrationEventsDispatcher :
        IConsumer<Published>,
        IConsumer<Canceled>,
        IConsumer<AttendantAdded>,
        IConsumer<AttendantMovedToWaiting>
    {
        private readonly GetMeetupDetails GetMeetupDetails;
        private readonly GetMeetupEventId GetMeetupId;

        public IntegrationEventsDispatcher(GetMeetupDetails getMeetupDetails, GetMeetupEventId getMeetupEventId)
        {
            GetMeetupDetails = getMeetupDetails;
            GetMeetupId      = getMeetupEventId;
        }

        public async Task Consume(ConsumeContext<Published> context)
        {
            await context.Publish(new V1.MeetupEventPublished(context.Message.Id, context.Message.At));

            var meetup = await GetMeetupDetails(context.Message.Id);
            if (meetup is null)
                throw new ArgumentException($"Meetup details {context.Message.Id} not found.");

            await context.Publish(new V2.MeetupEventPublished(context.Message.Id, meetup.Title, meetup.Description));
        }

        public Task Consume(ConsumeContext<Canceled> context) =>
            context.Publish(new V1.MeetupEventCanceled(context.Message.Id, context.Message.Reason,
                context.Message.At));

        public async Task Consume(ConsumeContext<AttendantAdded> context)
        {
            var meetupId = await GetMeetupEventId(context.Message.Id);
            await context.Publish(new V1.AttendantAdded(meetupId, context.Message.MemberId, context.Message.At));
        }

        public async Task Consume(ConsumeContext<AttendantMovedToWaiting> context)
        {
            var meetupId = await GetMeetupEventId(context.Message.Id);
            await context.Publish(new V1.AttendantMovedToWaiting(meetupId, context.Message.MemberId,
                context.Message.At));
        }

        async Task<Guid> GetMeetupEventId(Guid attendantListId)
        {
            var meetupId = await GetMeetupId(attendantListId);
            if (meetupId is null)
                throw new ArgumentException($"MeetupId for AttendantList {attendantListId} not found.");

            return meetupId.Value;
        }
    }
}