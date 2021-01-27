using System;
using System.Threading.Tasks;
using MeetupEvents.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace MeetupEvents.Queries
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventsQueryApi : ControllerBase
    {
        readonly MeetupEventsRepository Repository;

        public MeetupEventsQueryApi(MeetupEventsRepository repository)
        {
            Repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await Repository.GetAll());

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id) =>
            await Repository.Get(id) switch
            {
                null       => NotFound($"Meetup event {id} not found"),
                var meetup => Ok(meetup),
            };
    }
}