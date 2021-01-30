using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Infrastructure
{
    public class Repository<TAggregate> where TAggregate : Aggregate
    {
        protected readonly MeetupDbContext DbContext;

        public Repository(MeetupDbContext dbContext)
            => DbContext = dbContext;

        public virtual Task<TAggregate?> Load(Guid id)
            => DbContext.Set<TAggregate>().SingleOrDefaultAsync(x => x.Id == id)!;

        public async Task Add(TAggregate aggregate)
        {
            await DbContext.Set<TAggregate>().AddAsync(aggregate);
        }

        public async Task SaveChanges(TAggregate aggregate)
        {
            if (DbContext.ChangeTracker.HasChanges())
                aggregate.IncreaseVersion();

            await DbContext.SaveChangesAsync();
        }
    }
}