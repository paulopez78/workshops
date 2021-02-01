using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;

namespace MeetupEvents.Infrastructure
{
    public class OutboxMiddleware : IApplicationService
    {
        readonly IApplicationService AppService;
        readonly MeetupDbContext     DbContext;
        readonly IPublishEndpoint    PublishEndpoint;
        readonly IDateTimeProvider   DateTimeProvider;

        public OutboxMiddleware(IApplicationService applicationService, MeetupDbContext dbContext,
            IPublishEndpoint publishEndpoint, IDateTimeProvider dateTimeProvider)
        {
            AppService       = applicationService;
            DbContext        = dbContext;
            PublishEndpoint  = publishEndpoint;
            DateTimeProvider = dateTimeProvider;
        }

        public async Task<CommandResult> HandleCommand(Guid id, object command)
        {
            CommandResult result;
            List<Outbox>  outbox;

            // deduplication infrastructure here
            await using (var _ = await DbContext.Database.BeginTransactionAsync())
            {
                result = await AppService.HandleCommand(id, command);

                outbox = result.Changes.Select(x => Outbox.From(id, x)).ToList();

                await DbContext.Set<Outbox>().AddRangeAsync(outbox);

                await DbContext.SaveChangesAsync();
                await DbContext.Database.CommitTransactionAsync();
            }

            await Dispatch();

            return result;

            async Task Dispatch()
            {
                await Task.WhenAll(
                    outbox.Select(x => PublishEndpoint.Publish(x.DomainEvent))
                );

                foreach (var domainEvent in outbox)
                    domainEvent.DispatchedAt = DateTimeProvider.GetUtcNow();

                await DbContext.SaveChangesAsync();
            }
        }
    }

    public record Outbox
    {
        public Guid   Id          { get; set; }
        public Guid   AggregateId { get; set; }
        public string MessageType { get; set; }
        public string Payload     { get; set; }

        public DateTimeOffset? DispatchedAt { get; set; }
        public object          DomainEvent  => JsonSerializer.Deserialize(Payload, Type.GetType(MessageType));

        public static Outbox From(Guid aggregateId, object domainEvent) =>
            new()
            {
                AggregateId = aggregateId,
                Payload     = JsonSerializer.Serialize(domainEvent),
                MessageType = $"{domainEvent.GetType().FullName}, {domainEvent.GetType().Assembly.FullName}",
            };
    }
}