using System;
using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Framework;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.AttendantList
{
    public class MeetupCreatedMassTransitDomainEventHandler : IConsumer<MeetupDetails.Events.V1.Created>
    {
        readonly HandleCommand<AttendantListAggregate> Handle;

        readonly ILogger<MeetupCreatedMassTransitDomainEventHandler> Logger;

        public MeetupCreatedMassTransitDomainEventHandler(HandleCommand<AttendantListAggregate> handle,
            ILogger<MeetupCreatedMassTransitDomainEventHandler> logger)
        {
            Handle = handle;
            Logger = logger;
        }

        public async Task Consume(ConsumeContext<MeetupDetails.Events.V1.Created> context)
        {
            Logger.LogInformation("Executing meetup created handler");

            var meetupCreated = context.Message;
            var id          = Guid.NewGuid();

            await Handle
                .WithContext(context)
                .Invoke(id, new Commands.V1.Create(id, meetupCreated.Id, meetupCreated.Capacity));
        }
    }
}