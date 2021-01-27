using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MeetupEvents
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventsApi : ControllerBase
    {
        readonly MeetupEventPostgresDb _db;

        public MeetupEventsApi(MeetupEventPostgresDb db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _db.GetAll());

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id) =>
            await _db.Get(id) switch
            {
                null       => NotFound($"Meetup event {id} not found"),
                var meetup => Ok(meetup),
            };

        [HttpPost]
        public async Task<IActionResult> Create(MeetupEvent meetupEvent)
            => await _db.Add(meetupEvent)
                ? Ok()
                : BadRequest($"Meetup event {meetupEvent.Id} already exists");

        [HttpPut("{id:Guid}")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var meetup = await _db.Get(id);
            if (meetup is null) return NotFound($"Meetup event {id} not found");

            meetup.Published = true;

            await _db.SaveChanges();
            return Ok();
        }

        // _db.Update(id, meetup => meetup with { Published = true }) switch
        // {
        //     false => NotFound($"Meetup event {id} not found"),
        //     true => Ok()
        // };

        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> Cancel(Guid id)
            => await _db.Remove(id)
                ? Ok()
                : NotFound($"Meetup event {id} not found");
    }
}