using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Meetup.Scheduling.Infrastructure
{
    public class OutboxProcessor : BackgroundService
    {
        public OutboxProcessor(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        readonly IServiceProvider ServiceProvider;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessMessage(stoppingToken);
                await Task.Delay(50, stoppingToken);
            }
        }

        private async Task ProcessMessage(CancellationToken stoppingToken)
        {
            using var scope = ServiceProvider.CreateScope();

            var sp = scope.ServiceProvider;

            var dbContext  = sp.GetRequiredService<MeetupSchedulingDbContext>();
            var dispatcher = sp.GetRequiredService<DomainEventsDispatcher>();

            // improvement: we can take them in batches
            var outbox =
                await dbContext.Outbox.FirstOrDefaultAsync(x => !x.Dispatched, stoppingToken);

            if (outbox is null) return;

            var domainEvent = JsonSerializer.Deserialize(outbox.Payload, Type.GetType(outbox.DomainType)!);

            if (domainEvent is null) return;

            // should we retry when there is an error publishing?
            await dispatcher.Publish(sp, domainEvent);

            // we can't commit transaction if publish is not done without errors
            outbox.Dispatched = true;
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }

    public record Outbox(Guid Id, string DomainType, string Payload)
    {
        public bool Dispatched { get; set; }

        public static Outbox Map(object @event)
            => new(Guid.NewGuid(), @event.GetType().ToString(), JsonSerializer.Serialize(@event));
    }
}