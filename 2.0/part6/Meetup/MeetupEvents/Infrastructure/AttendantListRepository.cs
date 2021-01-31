using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Infrastructure
{
    public class AttendantListRepository : Repository<AttendantListAggregate>
    {
        public AttendantListRepository(MeetupDbContext dbContext, DomainEventsDispatcher dispatcher) :
            base(dbContext, dispatcher)
        {
        }

        public override Task<AttendantListAggregate?> Load(Guid id)
            => DbContext.Set<AttendantListAggregate>().Include(x => x.Attendants)
                .SingleOrDefaultAsync(x => x.Id == id)!;
    }
}