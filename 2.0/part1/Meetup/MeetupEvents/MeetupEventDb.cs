using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace MeetupEvents
{
    public class MeetupEventDb
    {
        readonly List<MeetupEvent>  _db = new();
        readonly MeetupEventOptions Options;

        public MeetupEventDb(IOptions<MeetupEventOptions> options)
        {
            Options = options.Value;
        }

        public IEnumerable<MeetupEvent> GetAll() => _db;

        public MeetupEvent? Get(Guid id)
        {
            var meetup = _db.SingleOrDefault(x => x.Id == id);
            return meetup;
        }

        public bool Add(MeetupEvent meetupEvent)
        {
            if (_db.Exists(x => x.Id == meetupEvent.Id))
                return false;

            if (meetupEvent.Capacity == 0)
                meetupEvent = meetupEvent with {Capacity = Options.DefaultCapacity};

            _db.Add(meetupEvent);
            return true;
        }

        public bool Remove(Guid id)
        {
            var affected = _db.RemoveAll(x => x.Id == id);
            return affected > 0;
        }

        public bool Update(Guid id, Func<MeetupEvent, MeetupEvent> getUpdate)
        {
            var meetup = Get(id);
            return meetup is not null && Remove(id) && Add(getUpdate(meetup));
        }
    }
}