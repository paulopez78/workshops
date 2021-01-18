using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImTools;
using Marten;
using Marten.Exceptions;
using MassTransit;
using Meetup.Scheduling.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Meetup.Scheduling.Shared;
using static System.Guid;

namespace Meetup.Scheduling.Infrastructure
{
    public interface IApplicationService
    {
        Task<CommandResult> Handle(Guid aggregateId, object command, CommandContext context);
    }

    public abstract class ApplicationService<T> : IApplicationService where T : Aggregate, new()
    {
        readonly IDocumentStore EventStore;

        protected ApplicationService(IDocumentStore eventStore)
            => EventStore = eventStore;

        public abstract Task<CommandResult> Handle(Guid aggregateId, object command, CommandContext context);

        protected async Task<CommandResult> Handle(Guid id, Action<T> command, CommandContext context)
        {
            var session = context.OpenDocumentSession(EventStore);

            var aggregate = await LoadAggregate(id, session);

            command(aggregate);

            session.Events.Append(aggregate.Id, aggregate.Version, aggregate.Changes);

            await Project(session, aggregate.Id, aggregate.Changes);

            context.Changes = aggregate.Changes.ToList();
            aggregate.ClearChanges();

            return new(aggregate.Id);
        }

        static async Task Project(IDocumentSession session, Guid id, IEnumerable<object> changes)
        {
            var state      = await session.LoadAsync<MeetupEvent>(id);
            var projection = changes.Aggregate(state, MeetupEventProjection.When);
            session.Delete(projection);
            session.Insert(projection);
        }

        async Task<T> LoadAggregate(Guid id, IDocumentSession session)
        {
            var events      = await session.Events.FetchStreamAsync(id);
            var streamState = await session.Events.FetchStreamStateAsync(id);

            var aggregate = new T {Id = id, Version = streamState?.Version ?? -1};

            foreach (var domainEvent in events.Select(x => x.Data))
                aggregate.When(domainEvent);

            return aggregate;
        }
    }

    public record CommandResult(Guid AggregateId);

    public class CommandContext
    {
        public Guid         MessageId { get; }
        public List<object> Changes   { get; set; }

        private IDocumentSession _documentSession;

        public IDocumentSession OpenDocumentSession(IDocumentStore store)
        {
            _documentSession ??= store.OpenSession();
            return _documentSession;
        }

        CommandContext(Guid messageId)
        {
            MessageId = messageId;
            Changes   = new List<object>();
        }

        public static CommandContext From(IHeaderDictionary context)
            => new(TryParse(context["Idempotency-Key"], out var result) ? result : NewGuid());

        public static CommandContext From(MessageContext context)
            => new(context.MessageId!.Value);
    }

    public static class ApplicationServiceExtensions
    {
        public static IApplicationService Build(this IApplicationService applicationService,
            IDocumentStore eventStore,
            IPublishEndpoint publishEndpoint,
            UtcNow getUtcNow)
        {
            return new OutboxMiddleware(applicationService, eventStore, publishEndpoint, getUtcNow);
        }

        public static Task HandleCommand(this IApplicationService applicationService,
            Guid aggregateId, object command, MessageContext context) =>
            applicationService.Handle(aggregateId, command, CommandContext.From(context));

        public static async Task<IActionResult> HandleCommand(this IApplicationService applicationService,
            Guid aggregateId, object command, IHeaderDictionary headers)
        {
            try
            {
                return new OkObjectResult(await applicationService.Handle(aggregateId, command,
                    CommandContext.From(headers)));
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
        }
    }
}