using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Meetup.Scheduling.AttendantList
{
    public class AttendantListRepository : MeetupRepository<AttendantListAggregate>
    {
        public AttendantListRepository(MeetupSchedulingDbContext dbContext):base(dbContext)
        {
        }

        public override Task<AttendantListAggregate?> Load(Guid id)
            => DbContext.Set<AttendantListAggregate>().Include(x => x.Attendants).SingleOrDefaultAsync(x => x.Id == id)!;
    }
}