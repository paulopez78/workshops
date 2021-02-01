using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.Scheduling
{
    public class MeetupEventRepository
    {
        readonly InMemoryDatabase Database;

        public MeetupEventRepository(InMemoryDatabase inMemoryDatabase) => Database = inMemoryDatabase;

        public MeetupEventEntity? Load(Guid id)
            => Database.MeetupEvents.FirstOrDefault(x => x.Id == id);

        public Guid Save(MeetupEventEntity entity)
        {
            if (Load(entity.Id) is null) Database.MeetupEvents.Add(entity);
            return entity.Id;
        }
    }

    public class InMemoryDatabase
    {
        public readonly List<MeetupEventEntity> MeetupEvents = new();
    }
}