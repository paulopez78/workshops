using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.Application.AttendantList
{
    public class MeetupCreatedDomainEventHandler : IDomainEventHandler<Events.V1.MeetupEvent.Created>
    {
        readonly IApplicationService ApplicationService;

        readonly ILogger<MeetupCreatedDomainEventHandler> Logger;

        public MeetupCreatedDomainEventHandler(AttendantListApplicationService applicationService,
            ILogger<MeetupCreatedDomainEventHandler> logger)
        {
            ApplicationService = applicationService;
            Logger             = logger;
        }

        public async Task Handle(Events.V1.MeetupEvent.Created @event)
        {
            Logger.LogInformation("Executing meetup created handler");
            
            // map event to a command
            // await ApplicationService.Handle(new Commands.V1.CreateAttendantList(@event.Id, DefaultCapacity));
        }
    }
}