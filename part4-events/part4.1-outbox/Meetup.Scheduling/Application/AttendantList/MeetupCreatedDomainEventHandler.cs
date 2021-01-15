using System;
using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Meetup.Scheduling.Application.AttendantList
{
    public class MeetupCreatedMassTransitDomainEventHandler : IConsumer<Events.V1.MeetupEvent.Created>
    {
        readonly IApplicationService ApplicationService;

        readonly ILogger<MeetupCreatedMassTransitDomainEventHandler> Logger;

        public MeetupCreatedMassTransitDomainEventHandler(AttendantListApplicationService applicationService,
            ILogger<MeetupCreatedMassTransitDomainEventHandler> logger)
        {
            ApplicationService = applicationService;
            Logger = logger;
        }

        public async Task Consume(ConsumeContext<Events.V1.MeetupEvent.Created> context)
        {
            Logger.LogInformation("Executing meetup created handler");

            var domainEvent = context.Message;

            // TransientError();

            try
            {
                await ApplicationService.HandleCommand(domainEvent.Id,
                    new Commands.V1.CreateAttendantList(domainEvent.Id, domainEvent.Capacity),
                    context);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "");
                throw;
            }
        }

        private void TransientError()
        {
            var r = new Random();

            if (r.Next(0, 10) > 5)
                throw new NpgsqlException("Fake error");
        }
    }
}