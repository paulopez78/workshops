using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static Meetup.Scheduling.Commands.V1;

namespace Meetup.Scheduling
{
    [Route("/api/meetup/{group}/events")]
    [ApiController]
    public class MeetupEventApi : ControllerBase
    {
        readonly MeetupEventApplicationService ApplicationService;
        readonly MeetupEventQueries Queries;
        readonly MeetupEventsOptions Options;

        public MeetupEventApi(
            MeetupEventApplicationService applicationService,
            MeetupEventQueries queries,
            IOptions<MeetupEventsOptions> options)
        {
            ApplicationService = applicationService;
            Queries = queries;
            Options = options.Value;
        }

        [HttpGet]
        public IActionResult GetAll(string group)
            => Ok(Queries.GetByGroup(group));

        [HttpGet("{eventId:guid}")]
        public IActionResult Get(Guid eventId)
        {
            var meetupEvent = Queries.Get(eventId);

            return meetupEvent is null
                ? NotFound($"MeetupEvent {eventId} not found")
                : Ok(meetupEvent);
        }

        [HttpPost]
        public IActionResult Post(string group, MeetupEvent meetupEvent)
        {
            var result = ApplicationService.Handle(
                new Create(
                    group,
                    meetupEvent.Title,
                    meetupEvent.Capacity == 0 ? Options.DefaultCapacity : meetupEvent.Capacity)
            );
            return Ok(result);
        }

        [HttpPut("{eventId:guid}/capacity/increase")]
        public IActionResult IncreaseCapacity(Guid eventId, int capacity)
        {
            ApplicationService.Handle(new IncreaseCapacity(eventId, capacity));
            return Ok();
        }

        [HttpPut("{eventId:guid}/capacity/reduce")]
        public IActionResult ReduceCapacity(Guid eventId, int capacity)
        {
            ApplicationService.Handle(new ReduceCapacity(eventId, capacity));
            return Ok();
        }

        [HttpPut("{eventId:guid}")]
        public IActionResult PublishEvent(Guid eventId)
        {
            ApplicationService.Handle(new Publish(eventId));
            return Ok();
        }

        [HttpDelete("{eventId:guid}")]
        public IActionResult CancelEvent(Guid eventId)
        {
            ApplicationService.Handle(new Cancel(eventId));
            return Ok();
        }

        [HttpPut("{eventId:guid}/invitations/accept")]
        public IActionResult Accept(Guid eventId, Guid userId)
        {
            // validation of request vs domain validation
            ApplicationService.Handle(new AcceptInvitation(eventId, userId));
            return Ok();
        }

        [HttpPut("{eventId:guid}/invitations/decline")]
        public IActionResult Decline(Guid eventId, Guid userId)
        {
            ApplicationService.Handle(new DeclineInvitation(eventId, userId));
            return Ok();
        }
    }

    public record MeetupEvent([Required] string Title, int Capacity = 100);

    public record MeetupEventsOptions
    {
        public int DefaultCapacity { get; init; }
    }
}