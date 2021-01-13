using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;

namespace Meetup.Scheduling.Application.AttendantList
{
    public class MeetupCanceledDomainEventHandler : IDomainEventHandler<Events.V1.MeetupEvent.Cancelled>
    {
        readonly IApplicationService ApplicationService;

        public MeetupCanceledDomainEventHandler(AttendantListApplicationService applicationService)
        {
            ApplicationService = applicationService;
        }

        public async Task Handle(Events.V1.MeetupEvent.Cancelled @event)
        {
            // map event to a command
            await ApplicationService.Handle(new Commands.V1.Close(@event.Id));
        }
    }
}