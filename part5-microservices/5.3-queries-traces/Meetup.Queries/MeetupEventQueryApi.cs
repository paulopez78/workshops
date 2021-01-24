using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Meetup.Queries
{
    [Route("/api/")]
    [ApiController]
    public class MeetupQueryApi : ControllerBase
    {
        readonly MeetupQueryHandler QueryHandler;

        public MeetupQueryApi(MeetupQueryHandler handler) => QueryHandler = handler;

        [HttpGet]
        [HttpGet("meetup/{group}")]
        public Task<IActionResult> GetByGroup([FromRoute] V1.GetMeetupGroup query)
            => QueryHandler.Handle(query);

        [HttpGet("meetup/{group}/{eventId:guid}")]
        public Task<IActionResult> GetById([FromRoute] V1.GetMeetupEvent query)
            => QueryHandler.Handle(query);

        [HttpGet("notifications//{userId:guid}")]
        public Task<IActionResult> GetById([FromRoute] V1.GetNotifications query)
            => QueryHandler.Handle(query);
    }
}