using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Marten.Exceptions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using static System.Guid;

namespace Meetup.Scheduling.Framework
{
    public delegate Task<IActionResult> HandleHttpCommand<out T>(Guid aggregateId, object command)
        where T : Aggregate, new();

    public delegate Task HandleAsyncCommand<out T>(Guid aggregateId, object command)
        where T : Aggregate, new();

    public delegate Task<CommandResult> HandleCommand<out T>(Guid aggregateId, object command, CommandContext context)
        where T : new();

    public delegate Task<CommandResult> Handle<out T>(Guid aggregateId, Action<T> command, CommandContext context)
        where T : Aggregate, new();

    public static class ApplicationServiceExtensions
    {
        public static Handle<T> AddApplicationService<T, TReadModel>(this IServiceProvider sp,
            Func<TReadModel, object, TReadModel> when) where T : Aggregate, new()
        {
            var eventStore      = sp.GetRequiredService<IDocumentStore>();
            var publishEndpoint = sp.GetRequiredService<IPublishEndpoint>();
            var getUtcNow       = sp.GetRequiredService<UtcNow>();
            return Build<T, TReadModel>(eventStore, publishEndpoint, getUtcNow, when);
        }

        static Handle<T> Build<T, TReadModel>(
            IDocumentStore eventStore,
            IPublishEndpoint publishEndpoint,
            UtcNow getUtcNow,
            Func<TReadModel, object, TReadModel> when)
            where T : Aggregate, new()
            => WithAggregate<T>()
                .WithProjection(when)
                .WithOutboxTransaction(eventStore, publishEndpoint, getUtcNow);

        static Handle<T> Build<T, TReadModel>(IDocumentStore eventStore, Func<TReadModel, object, TReadModel> when)
            where T : Aggregate, new()
            => WithAggregate<T>()
                .WithProjection(when)
                .WithTransaction(eventStore);

        public static Handle<T> WithTransaction<T>(this Handle<T> @this, IDocumentStore eventStore)
            where T : Aggregate, new() =>
            async (id, command, context) =>
            {
                using var session = eventStore.OpenSession();
                context.TransactionalSession = session;

                var result = await @this(id, command, context);
                return result;
            };

        public static Handle<T> WithAggregate<T>()
            where T : Aggregate, new() =>
            async (id, command, context) =>
            {
                var eventStore = context.TransactionalSession.Events;
                var aggregate  = await LoadAggregate();

                command(aggregate);

                eventStore.Append(aggregate.Id, aggregate.Version, aggregate.Changes);
                var result = new CommandResult(aggregate.Id, aggregate.Changes.ToList());

                aggregate.ClearChanges();
                return result;

                async Task<T> LoadAggregate()
                {
                    var events      = await eventStore.FetchStreamAsync(id);
                    var streamState = await eventStore.FetchStreamStateAsync(id);

                    var entity = new T {Id = id, Version = streamState?.Version ?? -1};

                    foreach (var domainEvent in events.Select(x => x.Data).ToList())
                        entity.When(domainEvent);

                    return entity;
                }
            };

        public static Handle<T> WithProjection<T, TReadModel>(this Handle<T> @this,
            Func<TReadModel, object, TReadModel> when)
            where T : Aggregate, new() =>
            async (id, command, context) =>
            {
                var result     = await @this(id, command, context);
                var state      = await context.TransactionalSession.LoadAsync<TReadModel>(id);
                var projection = result.Changes.Aggregate(state, when);
                context.TransactionalSession.Delete(projection);
                context.TransactionalSession.Insert(projection);

                return result;
            };

        public static HandleAsyncCommand<T> WithContext<T>(this HandleCommand<T> handle, ConsumeContext context)
            where T : Aggregate, new()
            => (aggregateId, command)
                => handle(aggregateId, command, CommandContext.From(context));

        public static HandleHttpCommand<T> WithContext<T>(this HandleCommand<T> handle, IHeaderDictionary headers)
            where T : Aggregate, new()
            => async (aggregateId, command) =>
            {
                try
                {
                    var result = await handle(aggregateId, command, CommandContext.From(headers));
                    return new OkObjectResult(result);
                }
                catch (ApplicationException e)
                {
                    return new BadRequestObjectResult(e.Message);
                }
                catch (ArgumentException e)
                {
                    return new BadRequestObjectResult(e.Message);
                }
                catch (EventStreamUnexpectedMaxEventIdException e)
                {
                    return new ObjectResult(e.Message)
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }
                catch (ConcurrencyException e)
                {
                    return new ObjectResult(e.Message)
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }
            };
    }

    public record CommandResult(Guid AggregateId, List<object> Changes);

    public class CommandContext
    {
        public Guid             MessageId            { get; }
        public IDocumentSession TransactionalSession { get; set; }

        CommandContext(Guid messageId) => MessageId = messageId;

        public static CommandContext From(IHeaderDictionary context)
            => new(TryParse(context["Idempotency-Key"], out var result) ? result : NewGuid());

        public static CommandContext From(MessageContext context)
            => new(context.MessageId!.Value);
    }
}