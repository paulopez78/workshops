using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.Infrastructure
{
    public class OutboxProcessor : BackgroundService
    {
        public OutboxProcessor(IServiceProvider serviceProvider, UtcNow now, ILogger<OutboxProcessor> logger)
        {
            ServiceProvider = serviceProvider;
            Now             = now;
            Logger          = logger;
        }

        readonly IServiceProvider         ServiceProvider;
        readonly UtcNow                   Now;
        readonly ILogger<OutboxProcessor> Logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMessage(stoppingToken);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error processing outbox");
                }

                await Task.Delay(10, stoppingToken);
            }
        }

        private async Task ProcessMessage(CancellationToken stoppingToken)
        {
            using var scope = ServiceProvider.CreateScope();

            var sp = scope.ServiceProvider;

            var dbContext       = sp.GetRequiredService<MeetupSchedulingDbContext>();
            var publishEndpoint = sp.GetRequiredService<IPublishEndpoint>();
            // var dispatcher = sp.GetRequiredService<DomainEventsDispatcher>();

            // improvement: we can take them in batches
            var outbox =
                await dbContext.Outbox.FirstOrDefaultAsync(x => x.DispatchedAt == null, stoppingToken);

            if (outbox is null) return;

            var domainEvent = JsonSerializer.Deserialize(outbox.Payload, Type.GetType(outbox.MessageType)!);
            if (domainEvent is null) return;

            // should we retry when there is an error publishing?
            // await dispatcher.Publish(sp, domainEvent);

            // dispatch to message broker using MassTransit library
            await publishEndpoint.Publish(domainEvent, stoppingToken);

            // we can't commit transaction if publish is not done without errors
            outbox.DispatchedAt = Now();
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}