using System;
using System.Linq;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;
using static Meetup.Scheduling.Infrastructure.Outbox;

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

        public async Task<Guid> Save(T aggregate)
        {
            var loadedAggregate = await Load(aggregate.Id);

            if (loadedAggregate is null)
                await DbContext.Set<T>().AddAsync(aggregate);

            if (DbContext.ChangeTracker.HasChanges())
                aggregate.IncreaseVersion();

            // dispatch domain events before transaction commit
            // await Task.WhenAll(aggregate.Changes.Select(Dispatcher.Publish));

            // Save domain events as part of the transaction
            await DbContext.Set<Outbox>().AddRangeAsync(
                aggregate.Changes.Select(Map)
            );
            await DbContext.SaveChangesAsync();

            // dispatch domain after transaction commit
            //await Task.WhenAll(aggregate.Changes.Select(Dispatcher.Publish));

            aggregate.ClearChanges();
            return aggregate.Id;
        }
    }
}