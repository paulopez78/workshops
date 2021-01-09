using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.Infrastructure
{
    public class MeetupEventDetailsRepository
    {
        readonly MeetupSchedulingDbContext _schedulingDbContext;
        private readonly ILogger<MeetupEventDetailsRepository> Logger;

        public MeetupEventDetailsRepository(MeetupSchedulingDbContext schedulingDbContext,
            ILogger<MeetupEventDetailsRepository> logger)
        {
            _schedulingDbContext = schedulingDbContext;
            Logger = logger;
        }

        public Task<MeetupEventDetailsAggregate?> Load(Guid id)
            => _schedulingDbContext.MeetupEventDetails.SingleOrDefaultAsync(x => x.Id == id)!;

        public async Task<Guid> Save(MeetupEventDetailsAggregate entity)
        {
            var dbEntity = await Load(entity.Id);

            if (dbEntity is null)
                await _schedulingDbContext.MeetupEventDetails.AddAsync(entity);

            if (_schedulingDbContext.ChangeTracker.HasChanges())
                entity.IncreaseVersion();

            await _schedulingDbContext.SaveChangesAsync();
            return entity.Id;
        }
    }
}