using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GreenPipes;
using Marten;
using MassTransit;
using static System.Guid;

namespace Meetup.Scheduling.Framework
{
    public static class OutboxExtensions
    {
        public static Handle<T> WithOutboxTransaction<T>(this Handle<T> @this, IDocumentStore eventStore,
            IPublishEndpoint publishEndpoint, UtcNow getUtcNow) where T : Aggregate, new() =>
            async (id, command, context) =>
            {
                var loadedOutbox = await LoadOutbox();

                if (loadedOutbox.Any())
                {
                    await Dispatch(loadedOutbox);
                    return new CommandResult(id, new());
                }

                var (result, savedOutbox) = await ExecuteOutboxTransaction();
                await Dispatch(savedOutbox);
                return result;

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
                                publishEndpoint.Publish(
                                    x.Change,
                                    new CustomMessageId(x.OutMessageId))
                            )
                    );

                async Task MarkAsDispatched(OutBox[] outbox)
                {
                    using var session = eventStore.OpenSession();

                    foreach (var item in outbox)
                    {
                        item.DispatchedAt = getUtcNow();
                    }

                    session.Store(outbox);
                    await session.SaveChangesAsync();
                }

                async Task<List<OutBox>> LoadOutbox()
                {
                    using var lightSession = eventStore.QuerySession();
                    var result = await lightSession.Query<OutBox>()
                        .Where(x => x.MessageId == context.MessageId && x.AggregateId == id)
                        .ToListAsync();

                    return result.ToList();
                }

                async Task<(CommandResult, List<OutBox>)> ExecuteOutboxTransaction()
                {
                    using var session = eventStore.OpenSession();
                    context.TransactionalSession = session;

                    // execute next handler 
                    var result = await @this(id, command, context);

                    var outboxMessages = result.Changes
                        .Select(@event => OutBox.From(context.MessageId, id, @event, NewGuid()))
                        .ToList();

                    session.Store<OutBox>(outboxMessages);
                    await session.SaveChangesAsync();
                    return (result, outboxMessages);
                }
            };
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