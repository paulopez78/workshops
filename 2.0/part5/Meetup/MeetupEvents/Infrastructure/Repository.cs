using System;
using System.Linq;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Infrastructure
{
    public class Repository<TAggregate> where TAggregate : Aggregate
    {
        protected readonly MeetupDbContext        DbContext;
        readonly           DomainEventsDispatcher Dispatcher;

        public Repository(MeetupDbContext dbContext, DomainEventsDispatcher dispatcher)
        {
            DbContext  = dbContext;
            Dispatcher = dispatcher;
        }

        public virtual Task<TAggregate?> Load(Guid id)
            => DbContext.Set<TAggregate>().SingleOrDefaultAsync(x => x.Id == id)!;

        public async Task Add(TAggregate aggregate) 
            => await DbContext.Set<TAggregate>().AddAsync(aggregate);

        public async Task SaveChanges(TAggregate aggregate)
        {
            if (DbContext.Database.CurrentTransaction is null)
            {
                await using var tx = await DbContext.Database.BeginTransactionAsync();
                await SingleTransaction();
                await DbContext.Database.CommitTransactionAsync();
            }
            else
            {
                await SingleTransaction();
            }

            async Task SingleTransaction()
            {
                if (DbContext.ChangeTracker.HasChanges())
                    aggregate.IncreaseVersion();

                // event handlers are part of the same transaction
                await Task.WhenAll(
                    aggregate.Changes.Select(Dispatcher.Publish)
                );

                aggregate.ClearChanges();
                await DbContext.SaveChangesAsync();
            }
        }
    }
}