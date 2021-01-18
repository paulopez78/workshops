using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private MeetupRepository<T> Repository;

        protected ApplicationService(MeetupRepository<T> repository)
            => Repository = repository;

        protected async Task<CommandResult> Handle(Guid id, Action<T> command, CommandContext context)
        {
            var aggregate = await Repository.Load(id) ?? new T {Id = id};
            command(aggregate);
            return await SaveChanges(aggregate, context);
        }

        async Task<CommandResult> SaveChanges(T aggregate, CommandContext context)
        {
            await Repository.Save(aggregate);

            context.PendingChanges = new List<object>(aggregate.Changes);
            aggregate.ClearChanges();

            return new(aggregate.Id);
        }

        public abstract Task<CommandResult> Handle(Guid aggregateId, object command, CommandContext context);
    }

    public record CommandResult(Guid AggregateId);

    public class CommandContext
    {
        public Guid                  MessageId      { get; }
        public IReadOnlyList<object> PendingChanges { get; set; }

        CommandContext(Guid messageId)
        {
            MessageId      = messageId;
            PendingChanges = new List<object>();
        }

        public static CommandContext From(IHeaderDictionary context)
            => new(Parse(context["Idempotency-Key"]));

        public static CommandContext From(MessageContext context)
            => new(context.MessageId!.Value);
    }

    public static class ApplicationServiceExtensions
    {
        public static IApplicationService Build(this IApplicationService applicationService,
            MeetupSchedulingDbContext dbContext,
            IPublishEndpoint publishEndpoint,
            UtcNow getUtcNow,
            ILogger logger)
            => new ExceptionHandlingLoggerMiddleware(
                new OutboxMiddleware(
                    applicationService, dbContext, publishEndpoint, getUtcNow
                ),
                logger
            );

        public static IApplicationService Build(this IApplicationService applicationService, ILogger logger) =>
            new ExceptionHandlingLoggerMiddleware(applicationService, logger);

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
            catch (DbUpdateConcurrencyException e)
            {
                return new ObjectResult(e.Message)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }

    public class OutboxMiddleware : IApplicationService
    {
        public OutboxMiddleware(IApplicationService appService, MeetupSchedulingDbContext dbContext,
            IPublishEndpoint publishEndpoint, UtcNow getUtcNow)
        {
            AppService      = appService;
            DbContext       = dbContext;
            PublishEndpoint = publishEndpoint;
            GetUtcNow       = getUtcNow;
        }

        readonly IApplicationService       AppService;
        readonly MeetupSchedulingDbContext DbContext;
        readonly IPublishEndpoint          PublishEndpoint;
        readonly UtcNow                    GetUtcNow;

        public async Task<CommandResult> Handle(Guid aggregateId, object cmd, CommandContext context)
        {
            // load aggregate outbox messages for incoming messageId
            var loadedOutbox = await DbContext.Outbox
                .Where(x => x.MessageId == context.MessageId && x.AggregateId == aggregateId)
                .ToListAsync();

            if (!loadedOutbox.Any())
            {
                // begin transaction
                using var tx = DbContext.Database.BeginTransactionAsync();

                // execute next handler 
                await AppService.Handle(aggregateId, cmd, context);

                // store outbox changes
                var outboxMessages = context.PendingChanges
                    .Select(change => OutBox.From(context.MessageId, aggregateId, change, NewGuid()))
                    .ToList();

                await DbContext.Outbox.AddRangeAsync(outboxMessages);
                await DbContext.SaveChangesAsync();

                // commit transaction
                await DbContext.Database.CommitTransactionAsync();

                await Dispatch(outboxMessages);
            }
            else
            {
                await Dispatch(loadedOutbox);
            }

            return new(aggregateId);

            async Task Dispatch(List<OutBox> outbox)
            {
                await TryDispatch(outbox);
                await MarkAsDispatched(outbox);
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

            Task MarkAsDispatched(IEnumerable<OutBox> outbox)
            {
                foreach (var item in outbox)
                {
                    item.DispatchedAt = GetUtcNow();
                }

                return DbContext.SaveChangesAsync();
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

    public record OutBox(Guid MessageId, Guid AggregateId, string MessageType, string Payload, Guid OutMessageId)
    {
        public DateTimeOffset? DispatchedAt { get; set; }
        public object?         Change       => JsonSerializer.Deserialize(Payload, Type.GetType(MessageType));

        public static OutBox From(Guid messageId, Guid aggregateId, object @event, Guid outMessageId)
            => new(messageId, aggregateId, @event.GetType().ToString(), JsonSerializer.Serialize(@event), outMessageId);
    }


    public class ExceptionHandlingLoggerMiddleware : IApplicationService
    {
        readonly ILogger             Logger;
        readonly IApplicationService ApplicationService;

        public ExceptionHandlingLoggerMiddleware(IApplicationService applicationService, ILogger logger)
        {
            ApplicationService = applicationService;
            Logger             = logger;
        }

        public async Task<CommandResult> Handle(Guid aggregateId, object command, CommandContext context)
        {
            try
            {
                Logger.LogDebug($"Executing {ApplicationService} command {command.GetType().FullName}");
                var result = await ApplicationService.Handle(aggregateId, command, context);
                Logger.LogDebug($"Executed {ApplicationService} command {command.GetType().FullName}");

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e,
                    $"Error executing application service {ApplicationService} with command {command.GetType().FullName}");
                throw;
            }
        }
    }
}