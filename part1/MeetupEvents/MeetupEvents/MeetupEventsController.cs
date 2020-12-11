using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MeetupEvents
{
    [Route("/api/meetup/events")]
    [ApiController]
    public class MeetupEventsController : ControllerBase
    {
        private MeetupEventsDb      _db;
        private MeetupEventsOptions _options;

        public MeetupEventsController(MeetupEventsDb db, MeetupEventsOptions options)
        {
            _db = db;
            _options = options;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_db.GetAll());
        }

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
                _db.Add(meetupEvent with { Capacity = _options.DefaultCapacity });
            else
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

    public record MeetupEventsOptions(int DefaultCapacity);
}