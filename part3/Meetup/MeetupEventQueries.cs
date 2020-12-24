using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.Scheduling
{
    public class MeetupEventQueries
    {
        readonly InMemoryDatabase Database;

        public MeetupEventQueries(InMemoryDatabase inMemoryDatabase) => Database = inMemoryDatabase;

        public MeetupEventEntity? Get(Guid id)
            => Database.MeetupEvents.FirstOrDefault(x => x.Id == id);
        
        public IEnumerable<MeetupEventEntity> GetByGroup(string group)
            => Database.MeetupEvents.Where(x => x.Group== group);
    }
}