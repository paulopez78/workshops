using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Infrastructure
{
    public class OutboxProcessor : BackgroundService
    {
        public OutboxProcessor(IServiceProvider serviceProvider, IDateTimeProvider dateTimeProvider,
            ILogger<OutboxProcessor> logger)
        {
            ServiceProvider = serviceProvider;
            DateTimeProvider = dateTimeProvider;
            Logger = logger;
        }

        readonly IServiceProvider ServiceProvider;
        readonly IDateTimeProvider DateTimeProvider;
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

                await Task.Delay(60_000, stoppingToken);
            }
        }

        private async Task ProcessMessage(CancellationToken stoppingToken)
        {
            using var scope = ServiceProvider.CreateScope();

            var sp = scope.ServiceProvider;

            var dbContext = sp.GetRequiredService<MeetupDbContext>();
            var publishEndpoint = sp.GetRequiredService<IPublishEndpoint>();

            var outbox = await dbContext.Set<Outbox>().Where(x => x.DispatchedAt == null).ToListAsync(stoppingToken);

            foreach (var message in outbox)
            {
                var domainEvent = JsonSerializer.Deserialize(message.Payload, Type.GetType(message.MessageType)!);
                if (domainEvent is null) return;

                await publishEndpoint.Publish(domainEvent, stoppingToken);

                message.DispatchedAt = DateTimeProvider.GetUtcNow();
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}