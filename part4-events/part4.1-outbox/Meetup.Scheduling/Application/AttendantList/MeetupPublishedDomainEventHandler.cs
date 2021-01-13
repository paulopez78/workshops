using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.Application.AttendantList
{
    // IDomainEventHandlers cant interact with third party services (http, smtp), they run inside same transaction
    public class MeetupPublishedDomainEventHandler : IDomainEventHandler<Events.V1.MeetupEvent.Published>
    {
        readonly IApplicationService ApplicationService;

        readonly ILogger<MeetupCreatedDomainEventHandler> Logger;

        public MeetupPublishedDomainEventHandler(AttendantListApplicationService applicationService,
            ILogger<MeetupCreatedDomainEventHandler> logger)
        {
            ApplicationService = applicationService;
            Logger             = logger;
        }

        public async Task Handle(Events.V1.MeetupEvent.Published @event)
        {
            Logger.LogInformation("Executing meetup published handler");
            
            // map event to a command
            await ApplicationService.Handle(new Commands.V1.Open(@event.Id));
        }
    }
}