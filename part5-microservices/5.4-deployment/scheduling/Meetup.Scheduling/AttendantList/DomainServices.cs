using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using static Meetup.Scheduling.Contracts.ReadModels.V1;

namespace Meetup.Scheduling.AttendantList
{
    public static class DomainServices
    {
        public static async Task<IEnumerable<Guid>> GetMeetupsWithOpenedList(this IDocumentStore store,
            string group)
        {
            using var session = store.QuerySession();
            var result = await session
                .Query<MeetupEvent>()
                .Where(x => x.Group == group &&
                            x.AttendantListStatus == AttendantListStatus.Opened.ToString())
                .ToListAsync();

            return result.Select(x => x.Id);
        }

        public static async Task<Guid?> GetAttendantListId(this IDocumentStore store, Guid meetupId)
        {
            using var session = store.QuerySession();
            var result = await session.Query<AttendantListReadModel>()
                .FirstOrDefaultAsync(x => x.MeetupEventId == meetupId);
            
            return result?.Id;
        }
    }
}