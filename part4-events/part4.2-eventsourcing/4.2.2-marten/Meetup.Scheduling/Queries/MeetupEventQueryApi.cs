using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Meetup.Scheduling.Queries
{
    [Route("/api/meetup/{group}/events")]
    [ApiController]
    public class MeetupEventQueryApi : ControllerBase
    {
        readonly MeetupEventPostgresQueries Queries;

        public MeetupEventQueryApi(MeetupEventPostgresQueries queries) => Queries = queries;

        [HttpGet]
        public async Task<IActionResult> GetByGroup([FromRoute] V1.GetByGroup query)
            => Ok(await Queries.Handle(query));

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] V1.GetById query)
        {
            var meetupEvent = await Queries.Handle(query);

            return meetupEvent is null
                ? NotFound($"MeetupEvent {query.EventId} not found")
                : Ok(meetupEvent);
        }
    }
}