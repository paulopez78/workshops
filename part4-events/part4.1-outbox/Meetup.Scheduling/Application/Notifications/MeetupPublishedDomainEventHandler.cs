using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Domain;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.Application.Notifications
{
    public class MeetupPublishedMassTransitDomainEventHandler : IConsumer<Events.V1.MeetupEvent.Published>
    {
        readonly ILogger<MeetupPublishedMassTransitDomainEventHandler> Logger;

        public MeetupPublishedMassTransitDomainEventHandler(ILogger<MeetupPublishedMassTransitDomainEventHandler> logger)
        {
            Logger = logger;
        }

        public async Task Consume(ConsumeContext<Events.V1.MeetupEvent.Published> context)
        {
            Logger.LogInformation("NOTIFICATION SENT!");

            // map event to a command
        }
    }
}