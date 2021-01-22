using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Saga;
using static Meetup.Scheduling.Contracts.MeetupDetailsEvents.V1;
using static Meetup.Scheduling.Contracts.MeetupDetailsCommands.V1;
using static Meetup.Scheduling.Contracts.AttendantListEvents.V1;
using static Meetup.Scheduling.Contracts.AttendantListCommands.V1;
using static Meetup.Scheduling.Contracts.IntegrationEvents.V1;

namespace Meetup.Scheduling.ProcessManager
{
    public class MeetupProcessManager : ISaga, IConsumer<Published>
    {
        public Guid CorrelationId { get; set; }
        
        public Task Consume(ConsumeContext<Published> context)
        {
            throw new NotImplementedException();
        }
    }
}