using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.AttendantList
{
    public class MeetupPublishedMassTransitDomainEventHandler : IConsumer<MeetupDetails.Events.V1.Published>
    {
        readonly IApplicationService ApplicationService;

        readonly ILogger<MeetupPublishedMassTransitDomainEventHandler> Logger;

        public MeetupPublishedMassTransitDomainEventHandler(AttendantListApplicationService applicationService,
            ILogger<MeetupPublishedMassTransitDomainEventHandler> logger)
        {
            ApplicationService = applicationService;
            Logger = logger;
        }

        public async Task Consume(ConsumeContext<MeetupDetails.Events.V1.Published> context)
        {
            Logger.LogInformation("Executing meetup published handler");

            var message = context.Message;

            await ApplicationService.HandleCommand(message.Id, new Commands.V1.Open(message.Id), context);
        }
    }
}