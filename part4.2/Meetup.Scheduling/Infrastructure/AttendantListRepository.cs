using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.Infrastructure
{
    public class AttendantListRepository
    {
        readonly MeetupSchedulingDbContext _schedulingDbContext;

        public AttendantListRepository(MeetupSchedulingDbContext schedulingDbContext) =>
            _schedulingDbContext = schedulingDbContext;

        public Task<AttendantListAggregate?> Load(Guid id)
            => _schedulingDbContext.AttendantList.Include(x => x.AttendantList.Attendants)
                .SingleOrDefaultAsync(x => x.Id == id)!;

        async Task<bool> Exists(Guid id)
        {
            var result = await _schedulingDbContext.AttendantList.SingleOrDefaultAsync(x => x.Id == id);
            return result != default;
        }

        public async Task<Guid> Save(AttendantListAggregate entity)
        {
            if (!(await Exists(entity.Id)))
                await _schedulingDbContext.AttendantList.AddAsync(entity);

            if (_schedulingDbContext.ChangeTracker.HasChanges())
                entity.IncreaseVersion();

            await _schedulingDbContext.SaveChangesAsync();
            return entity.Id;
        }
    }
}