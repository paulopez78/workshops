using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GreenPipes;
using Marten;
using MassTransit;
using static System.Guid;

namespace Meetup.Scheduling.Infrastructure
{
    public class OutboxMiddleware : IApplicationService
    {
        public OutboxMiddleware(IApplicationService appService, IDocumentStore eventStore,
            IPublishEndpoint publishEndpoint, UtcNow getUtcNow)
        {
            AppService      = appService;
            EventStore      = eventStore;
            PublishEndpoint = publishEndpoint;
            GetUtcNow       = getUtcNow;
        }

        readonly IApplicationService AppService;
        readonly IDocumentStore      EventStore;
        readonly IPublishEndpoint    PublishEndpoint;
        readonly UtcNow              GetUtcNow;

        public async Task<CommandResult> Handle(Guid aggregateId, object cmd, CommandContext context)
        {
            var loadedOutbox = await LoadOutbox();

            if (loadedOutbox.Any())
            {
                await Dispatch(loadedOutbox);
            }
            else
            {
                var savedOutbox = await SaveOutbox();
                await Dispatch(savedOutbox);
            }

            return new(aggregateId);

            async Task Dispatch(IReadOnlyList<OutBox> outbox)
            {
                await TryDispatch(outbox);
                await MarkAsDispatched(outbox.ToArray());
            }

            Task TryDispatch(IEnumerable<OutBox> outbox)
                => Task.WhenAll(
                    outbox
                        .Where(x => x.DispatchedAt == null)
                        .Select(x =>
                            PublishEndpoint.Publish(
                                x.Change,
                                new CustomMessageId(x.OutMessageId))
                        )
                );

            async Task MarkAsDispatched(OutBox[] outbox)
            {
                using var session = EventStore.OpenSession();

                foreach (var item in outbox)
                {
                    item.DispatchedAt = GetUtcNow();
                }

                session.Store(outbox);
                await session.SaveChangesAsync();
            }

            async Task<List<OutBox>> LoadOutbox()
            {
                using var lightSession = EventStore.QuerySession();
                var result = await lightSession.Query<OutBox>()
                    .Where(x => x.MessageId == context.MessageId && x.AggregateId == aggregateId)
                    .ToListAsync();

                return result.ToList();
            }

            async Task<List<OutBox>> SaveOutbox()
            {
                using var session = context.OpenDocumentSession(EventStore);

                // execute next handler 
                await AppService.Handle(aggregateId, cmd, context);

                var outboxMessages = context.Changes
                    .Select(@event => OutBox.From(context.MessageId, aggregateId, @event, NewGuid()))
                    .ToList();

                session.Store<OutBox>(outboxMessages);
                await session.SaveChangesAsync();
                return outboxMessages;
            }
        }
    }

    public class CustomMessageId : IPipe<SendContext>
    {
        private readonly Guid MessageId;
        public CustomMessageId(Guid messageId) => MessageId = messageId;

        public Task Send(SendContext context)
        {
            context.MessageId = MessageId;
            return Task.CompletedTask;
        }

        public void Probe(ProbeContext context)
        {
        }
    }

    public record OutBox(Guid Id, Guid MessageId, Guid AggregateId, string MessageType, string Payload,
        Guid OutMessageId)
    {
        public DateTimeOffset? DispatchedAt { get; set; }
        public object?         Change       => JsonSerializer.Deserialize(Payload, Type.GetType(MessageType));

        public static OutBox From(Guid messageId, Guid aggregateId, object @event, Guid outMessageId)
            => new(NewGuid(), messageId, aggregateId, @event.GetType().ToString(), JsonSerializer.Serialize(@event),
                outMessageId);
    }
}