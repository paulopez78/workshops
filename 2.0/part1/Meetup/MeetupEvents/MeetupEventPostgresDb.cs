using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MeetupEvents
{
    public class MeetupEventPostgresDb
    {
        readonly MeetupEventOptions   Options;
        readonly MeetupEventDbContext DbContext;

        public MeetupEventPostgresDb(MeetupEventDbContext dbContext, IOptions<MeetupEventOptions> options)
        {
            Options   = options.Value;
            DbContext = dbContext;
        }

        public Task<List<MeetupEvent>> GetAll()
            => DbContext.MeetupEvents.ToListAsync();

        public Task<MeetupEvent?> Get(Guid id)
            => DbContext.MeetupEvents.SingleOrDefaultAsync(x => x.Id == id)!;

        public async Task<bool> Add(MeetupEvent meetupEvent)
        {
            // like primary key check
            var meetup = await Get(meetupEvent.Id);
            if (meetup is not null) return false;

            if (meetupEvent.Capacity == 0)
                meetupEvent = meetupEvent with {Capacity = Options.DefaultCapacity};

            await DbContext.MeetupEvents.AddAsync(meetupEvent);

            return await SaveChanges();
        }

        public async Task<bool> Remove(Guid id)
        {
            var meetup = await Get(id);
            if (meetup is null) return false;

            DbContext.MeetupEvents.Remove(meetup);

            var affected = await DbContext.SaveChangesAsync();
            return affected > 0;
        }

        public async Task<bool> SaveChanges()
        {
            var affected = await DbContext.SaveChangesAsync();
            return affected > 0;
        }
    }
}