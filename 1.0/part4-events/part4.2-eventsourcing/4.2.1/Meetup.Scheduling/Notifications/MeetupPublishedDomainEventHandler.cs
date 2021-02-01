using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using static Meetup.Scheduling.MeetupDetails.Events.V1;

namespace Meetup.Scheduling.Notifications
{
    public class MeetupPublishedMassTransitDomainEventHandler : IConsumer<Published>
    {
        readonly ILogger<MeetupPublishedMassTransitDomainEventHandler> Logger;

        public MeetupPublishedMassTransitDomainEventHandler(ILogger<MeetupPublishedMassTransitDomainEventHandler> logger)
        {
            Logger = logger;
        }

        public async Task Consume(ConsumeContext<Published> context)
        {
            Logger.LogInformation("NOTIFICATION SENT!");

            // map event to a command
        }
    }
}