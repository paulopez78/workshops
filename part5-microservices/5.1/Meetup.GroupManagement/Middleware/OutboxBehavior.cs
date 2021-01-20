using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Meetup.GroupManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace Meetup.GroupManagement.Middleware
{
    public class OutboxBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        readonly DbContext        DbContext;
        readonly IPublishEndpoint PublishEndpoint;

        public OutboxBehavior(MeetupGroupManagementDbContext dbContext, IPublishEndpoint publishEndpoint)
        {
            DbContext       = dbContext;
            PublishEndpoint = publishEndpoint;
        }

        public async Task<TResponse> Handle(TRequest request
            , CancellationToken cancellationToken
            , RequestHandlerDelegate<TResponse> next
        )
        {
            using var tx = DbContext.Database.BeginTransactionAsync(cancellationToken);

            var result = await next();

            var pendingOutbox = DbContext.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added)
                .Select(x => x.Entity)
                .OfType<Outbox>()
                .ToList();

            await DbContext.SaveChangesAsync(cancellationToken);
            await DbContext.Database.CommitTransactionAsync(cancellationToken);

            await TryDispatch();
            await MarkAsDispatched();

            return result;

            Task TryDispatch()
                => Task.WhenAll(
                    pendingOutbox
                        .Where(x => x.DispatchedAt == null)
                        .Select(x => PublishEndpoint.Publish(x.Change, cancellationToken))
                );

            Task MarkAsDispatched()
            {
                foreach (var item in pendingOutbox)
                {
                    item.DispatchedAt = DateTimeOffset.UtcNow;
                }

                return DbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public record Outbox(Guid EntityId, string MessageType, string Payload)
    {
        public DateTimeOffset? DispatchedAt { get; set; }

        public static Outbox From(Guid entityId, object notification)
            => new(entityId, notification.GetType().ToString(), JsonSerializer.Serialize(notification));

        public object? Change => JsonSerializer.Deserialize(Payload, Type.GetType(MessageType));
    }
}