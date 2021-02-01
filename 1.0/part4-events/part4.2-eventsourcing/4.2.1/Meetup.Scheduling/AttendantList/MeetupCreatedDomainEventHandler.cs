using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Meetup.Scheduling.Infrastructure;

namespace Meetup.Scheduling.AttendantList
{
    public class MeetupCreatedMassTransitDomainEventHandler : IConsumer<MeetupDetails.Events.V1.Created>
    {
        readonly IApplicationService ApplicationService;

        readonly ILogger<MeetupCreatedMassTransitDomainEventHandler> Logger;

        public MeetupCreatedMassTransitDomainEventHandler(AttendantListApplicationService applicationService,
            ILogger<MeetupCreatedMassTransitDomainEventHandler> logger)
        {
            ApplicationService = applicationService;
            Logger             = logger;
        }

        public async Task Consume(ConsumeContext<MeetupDetails.Events.V1.Created> context)
        {
            Logger.LogInformation("Executing meetup created handler");

            var domainEvent = context.Message;

            await ApplicationService.HandleCommand(domainEvent.Id,
                new Commands.V1.CreateAttendantList(domainEvent.Id, domainEvent.Capacity),
                context);
        }
    }
}