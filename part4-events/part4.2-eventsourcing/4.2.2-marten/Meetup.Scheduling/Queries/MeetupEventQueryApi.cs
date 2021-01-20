using System.Threading.Tasks;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Meetup.Scheduling.Queries
{
    [Route("/api/meetup/{group}/events")]
    [ApiController]
    public class MeetupEventQueryApi : ControllerBase
    {
        readonly IDocumentStore Store;

        public MeetupEventQueryApi(IDocumentStore store) => Store = store;

        [HttpGet]
        public async Task<IActionResult> GetByGroup([FromRoute] V1.GetByGroup query)
            => Ok(await Store.Handle(query));

        [HttpGet("{eventId:guid}")]
        public async Task<IActionResult> GetById([FromRoute] V1.GetById query)
        {
            //var meetupEvent = await Store.Handle(query);
            var meetupEvent = await Store.HandleWithProjection(query);

            return meetupEvent is null
                ? NotFound($"MeetupEvent {query.EventId} not found")
                : Ok(meetupEvent);
        }
    }
}