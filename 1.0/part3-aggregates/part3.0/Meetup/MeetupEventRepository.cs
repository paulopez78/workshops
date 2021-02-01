using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling
{
    public class MeetupEventRepository : IRepository
    {
        readonly InMemoryDatabase DbContext;

        public MeetupEventRepository(InMemoryDatabase inMemoryDatabase) => DbContext = inMemoryDatabase;

        public Task<Domain.MeetupEvent?> Load(Guid id) => Task.FromResult(GetEntity(id));

        Domain.MeetupEvent? GetEntity(Guid id) => DbContext.MeetupEvents.FirstOrDefault(x => x.Id == id);

        public Task<Guid> Save(Domain.MeetupEvent entity)
        {
            if (GetEntity(entity.Id) is null) DbContext.MeetupEvents.Add(entity);
            return Task.FromResult(entity.Id);
        }
    }

    public class MeetupEventPostgresRepository : IRepository
    {
        readonly         MeetupDbContext                        DbContext;
        private readonly ILogger<MeetupEventPostgresRepository> Logger;

        public MeetupEventPostgresRepository(MeetupDbContext dbContext, ILogger<MeetupEventPostgresRepository> logger)
        {
            DbContext = dbContext;
            Logger = logger;
        }

        public Task<Domain.MeetupEvent?> Load(Guid id)
            => DbContext.MeetupEvents.Include(x => x.Attendants).SingleOrDefaultAsync(x => x.Id == id)!;

        public async Task<Guid> Save(Domain.MeetupEvent entity)
        {
            var dbEntity = await Load(entity.Id);

            if (dbEntity is null)
                await DbContext.MeetupEvents.AddAsync(entity);

            if (DbContext.ChangeTracker.HasChanges())
                entity.IncreaseVersion();

            await DbContext.SaveChangesAsync();
            return entity.Id;
        }
    }

    public interface IRepository
    {
        Task<Domain.MeetupEvent?> Load(Guid id);
        Task<Guid> Save(Domain.MeetupEvent entity);
    }

    public class InMemoryDatabase
    {
        public readonly List<Domain.MeetupEvent> MeetupEvents = new();
    }
}