using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;

namespace Meetup.Scheduling.Application.AttendantList
{
    public class MeetupCanceledMassTransitDomainEventHandler : IConsumer<Events.V1.MeetupEvent.Cancelled>
    {
        readonly AttendantListApplicationService ApplicationService;

        public MeetupCanceledMassTransitDomainEventHandler(AttendantListApplicationService applicationService)
        {
            ApplicationService = applicationService;
        }

        public async Task Consume(ConsumeContext<Events.V1.MeetupEvent.Cancelled> context)
        {
            // map event to a command
            var cancelledEvent = context.Message;

            await ApplicationService.HandleCommand(
                cancelledEvent.Id,
                new Commands.V1.Close(cancelledEvent.Id),
                context
            );
        }
    }
}