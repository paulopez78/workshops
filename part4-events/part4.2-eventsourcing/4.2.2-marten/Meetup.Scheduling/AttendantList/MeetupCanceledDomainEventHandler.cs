using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Infrastructure;

namespace Meetup.Scheduling.AttendantList
{
    public class MeetupCanceledMassTransitDomainEventHandler : IConsumer<MeetupDetails.Events.V1.Cancelled>
    {
        readonly AttendantListApplicationService ApplicationService;

        public MeetupCanceledMassTransitDomainEventHandler(AttendantListApplicationService applicationService)
        {
            ApplicationService = applicationService;
        }

        public async Task Consume(ConsumeContext<MeetupDetails.Events.V1.Cancelled> context)
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