using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static MeetupEvents.Contracts.Queries.V1;

namespace MeetupEvents.Queries
{
    [ApiController]
    [Route("/api/meetup")]
    public class MeetupEventsQueryApi : ControllerBase
    {
        readonly MeetupEventQueries Queries;

        public MeetupEventsQueryApi(MeetupEventQueries queries) => Queries= queries;

        [HttpGet("{group:Guid}")]
        public async Task<IActionResult> GetByGroup(Guid groupId)
            => Ok(
                await Queries.Handle(new GetByGroup(groupId))
            );

        [HttpGet("events/{id:Guid}")]
        public async Task<IActionResult> Get(Guid id) =>
            await Queries.Handle(new Get(id))
                switch
                {
                    null       => NotFound($"Meetup event {id} not found"),
                    var meetup => Ok(meetup),
                };
    }
}