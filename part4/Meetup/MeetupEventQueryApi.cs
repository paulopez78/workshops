using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Data;
using Microsoft.AspNetCore.Mvc;

namespace Meetup.Scheduling
{
    [Route("/api/meetup/{group}/events")]
    [ApiController]
    public class MeetupEventQueryApi : ControllerBase
    {
        readonly MeetupEventPostgresQueries Queries;

        public MeetupEventQueryApi(MeetupEventPostgresQueries queries) => Queries = queries;

        [HttpGet]
        public async Task<IActionResult> GetAll(string group)
            => Ok(await Queries.GetByGroup(group));

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> Get(Guid eventId)
        {
            var meetupEvent = await Queries.Get(eventId);

            return meetupEvent is null
                ? NotFound($"MeetupEvent {eventId} not found")
                : Ok(meetupEvent);
        }
    }
}