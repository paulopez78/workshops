using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MeetupEvents
{
    public class MeetupEventsDb
    {
        Dictionary<int, MeetupEvent> MeetupEvents { get; }

        public MeetupEventsDb() => MeetupEvents = new();

        public void Add(MeetupEvent meetupEvent) =>
            MeetupEvents.Add(MeetupEvents.Count + 1, meetupEvent);

        public void Publish(int id)
        {
            var value = Get(id);
            if (value is not null) MeetupEvents[id] = value with { Published = true };
        }

        public void Remove(int id) => MeetupEvents.Remove(id);

        public MeetupEvent? Get(int id) => MeetupEvents.TryGetValue(id, out var value) ? value : null;

        public List<MeetupEvent> GetAll() => MeetupEvents.Select(x => x.Value).ToList();
    }

    public record MeetupEvent([Required] string Title, int Capacity = 100, bool Published = false);
}