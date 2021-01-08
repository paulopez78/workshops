using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.Infrastructure
{
    public class AttendantListRepository
    {
        readonly         MeetupSchedulingDbContext        _schedulingDbContext;
        private readonly ILogger<AttendantListRepository> Logger;

        public AttendantListRepository(MeetupSchedulingDbContext schedulingDbContext,
            ILogger<AttendantListRepository> logger)
        {
            _schedulingDbContext = schedulingDbContext;
            Logger               = logger;
        }

        public Task<AttendantList?> Load(Guid id)
            => _schedulingDbContext.AttendantList.Include(x => x.Attendants).SingleOrDefaultAsync(x => x.Id == id)!;

        public async Task<Guid> Save(AttendantList entity)
        {
            var dbEntity = await Load(entity.Id);

            if (dbEntity is null)
                await _schedulingDbContext.AttendantList.AddAsync(entity);

            if (_schedulingDbContext.ChangeTracker.HasChanges())
                entity.IncreaseVersion();

            await _schedulingDbContext.SaveChangesAsync();
            return entity.Id;
        }
    }
}