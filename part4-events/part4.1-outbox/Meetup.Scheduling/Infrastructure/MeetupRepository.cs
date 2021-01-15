using System;
using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;

namespace Meetup.Scheduling.Infrastructure
{
    public class MeetupRepository<T> where T : Aggregate
    {
        readonly MeetupSchedulingDbContext DbContext;

        public MeetupRepository(MeetupSchedulingDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public Task<T?> Load(Guid id)
            => DbContext.Set<T>().SingleOrDefaultAsync(x => x.Id == id)!;

        public async Task Save(T aggregate)
        {
            var loadedAggregate = await Load(aggregate.Id);

            if (loadedAggregate is null)
                await DbContext.Set<T>().AddAsync(aggregate);

            if (DbContext.ChangeTracker.HasChanges())
                aggregate.IncreaseVersion();

            // BEFORE: dispatch domain events before transaction commit
            // await Task.WhenAll(aggregate.Changes.Select(Dispatcher.Publish));

            // Save domain events as part of the transaction
            // await DbContext.Set<OutBox>().AddRangeAsync(
            //     aggregate.Changes.Select(x => Outbox.From(aggregate.Id, x))
            // );

            await DbContext.SaveChangesAsync();

            // AFTER: dispatch domain after transaction commit
            //await Task.WhenAll(aggregate.Changes.Select(Dispatcher.Publish));

            // Dispatch straight to message broker
            // await Task.WhenAll(aggregate.Changes.Select(x => PublishEndpoint.Publish(x)));
        }
    }
}