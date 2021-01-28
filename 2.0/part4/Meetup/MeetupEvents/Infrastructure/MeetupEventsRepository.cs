using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Infrastructure
{
    public class MeetupEventsRepository
    {
        readonly MeetupEventDbContext DbContext;

        public MeetupEventsRepository(MeetupEventDbContext dbContext)
            => DbContext = dbContext;

        public Task<MeetupEventAggregate?> Get(Guid id)
            => DbContext.MeetupEvents.Include(x => x.Attendants).SingleOrDefaultAsync(x => x.Id == id)!;

        public async Task Add(MeetupEventAggregate meetupEvent)
        {
            await DbContext.MeetupEvents.AddAsync(meetupEvent);
        }

        public async Task SaveChanges(MeetupEventAggregate aggregate)
        {
            if (DbContext.ChangeTracker.HasChanges())
                aggregate.IncreaseVersion();

            await DbContext.SaveChangesAsync();
        }
    }

    public record MeetupEventOptions()
    {
        public int DefaultCapacity { get; init; } = 100;
    }
}