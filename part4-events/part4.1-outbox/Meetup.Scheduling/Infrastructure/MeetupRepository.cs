using System;
using System.Linq;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;

namespace Meetup.Scheduling.Infrastructure
{
    public class MeetupRepository<T> where T : Aggregate
    {
        readonly MeetupSchedulingDbContext DbContext;
        readonly DomainEventsDispatcher    Dispatcher;

        public MeetupRepository(MeetupSchedulingDbContext dbContext, DomainEventsDispatcher dispatcher)
        {
            DbContext  = dbContext;
            Dispatcher = dispatcher;
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
            // all handlers run inside same transaction, only for persistence with ACID support
            // too many handlers will make transaction bigger
            
            //await Task.WhenAll(aggregate.Changes.Select(Dispatcher.Publish));

            await DbContext.SaveChangesAsync();
            
            // dispatch domain after transaction commit
            await Task.WhenAll(aggregate.Changes.Select(Dispatcher.Publish));
            return aggregate.Id;
        }
    }
}