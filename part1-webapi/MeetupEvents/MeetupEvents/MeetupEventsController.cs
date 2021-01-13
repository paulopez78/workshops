using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MeetupEvents
{
    [Route("/api/meetup/events")]
    [ApiController]
    public class MeetupEventsController : ControllerBase
    {
        private readonly MeetupEventsDb      _db;
        private readonly MeetupEventsOptions _options;

        public MeetupEventsController(MeetupEventsDb db, IOptions<MeetupEventsOptions> options)
        {
            _db = db;
            _options = options.Value;
        }

        [HttpGet]
        public IActionResult Get() => Ok(_db.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var meetupEvent = _db.Get(id);

            return meetupEvent is null
                ? NotFound($"MeetupEvent {id} not found")
                : Ok();
        }

        [HttpPost]
        public IActionResult CreateEvent(MeetupEvent meetupEvent)
        {
            if (meetupEvent.Capacity == 0)
                meetupEvent = meetupEvent with { Capacity = _options.DefaultCapacity };

            _db.Add(meetupEvent);

            return Ok();
        }

        [HttpPut]
        public IActionResult PublishEvent(int id)
        {
            _db.Publish(id);
            return Ok();
        }

        [HttpDelete]
        public IActionResult CancelEvent(int id)
        {
            _db.Remove(id);
            return Ok();
        }
    }

    public record MeetupEventsOptions
    {
        public int DefaultCapacity { get; init; }
    }
}