using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Shared;
using Microsoft.EntityFrameworkCore;

namespace Meetup.Scheduling.Infrastructure
{
    public class MeetupRepository<T> where T : Aggregate
    {
        protected readonly MeetupSchedulingDbContext DbContext;

        public MeetupRepository(MeetupSchedulingDbContext dbContext) => DbContext = dbContext;

        public virtual Task<T?> Load(Guid id)
            => DbContext.Set<T>().SingleOrDefaultAsync(x => x.Id == id)!;

        public async Task Save(T aggregate)
        {
            var loadedAggregate = await Load(aggregate.Id);

            if (loadedAggregate is null)
                await DbContext.Set<T>().AddAsync(aggregate);

            if (DbContext.ChangeTracker.HasChanges())
                aggregate.IncreaseVersion();

            await DbContext.SaveChangesAsync();
        }
    }
}