using System;
using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Framework;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.AttendantList
{
    public class MeetupCreatedDomainEventHandler : IConsumer<MeetupDetails.Events.V1.Created>
    {
        readonly HandleCommand<AttendantListAggregate> Handle;

        readonly ILogger<MeetupCreatedDomainEventHandler> Logger;

        public MeetupCreatedDomainEventHandler(HandleCommand<AttendantListAggregate> handle,
            ILogger<MeetupCreatedDomainEventHandler> logger)
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
                .Invoke(id, new Commands.V1.CreateAttendantList(id, meetupCreated.Id, meetupCreated.Capacity));
        }
    }
}