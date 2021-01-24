using System;
using System.Threading.Tasks;
using Marten;
using Microsoft.AspNetCore.Mvc;

namespace Meetup.Scheduling.Queries
{
    [Route("/api/meetup/{group}")]
    [ApiController]
    public class MeetupEventQueryApi : ControllerBase
    {
        readonly IDocumentStore Store;

        public MeetupEventQueryApi(IDocumentStore store) => Store = store;

        [HttpGet]
        public async Task<IActionResult> GetByGroup(string group)
            => Ok(await Store.HandleWithAsyncProjection(new V1.GetByGroup(group)));

        [HttpGet("events/{eventId:guid}")]
        public async Task<IActionResult> GetById(string group, Guid eventId)
        {
            //var meetupEvent = await Store.Handle(query);
            var meetupEvent = await Store.HandleWithAsyncProjection(new V1.GetById(eventId));

            return meetupEvent is null
                ? NotFound($"MeetupEvent {eventId} not found")
                : Ok(meetupEvent);
        }
    }
}