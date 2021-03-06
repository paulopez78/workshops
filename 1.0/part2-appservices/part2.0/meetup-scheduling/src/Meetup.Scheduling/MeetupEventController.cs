using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Meetup.Scheduling
{
    [Route("/api/meetup/{group}/events")]
    [ApiController]
    public class MeetupEventController : ControllerBase
    {
        private readonly MeetupApplicationService _applicationService;
        private readonly MeetupEventsOptions _options;

        public MeetupEventController(MeetupApplicationService applicationService, IOptions<MeetupEventsOptions> options)
        {
            _applicationService = applicationService;
            _options = options.Value;
        }

        [HttpGet]
        public IActionResult GetAll(string group) => Ok(_applicationService.GetAll(group));

        [HttpGet("{eventId:guid}")]
        public IActionResult Get(Guid eventId)
        {
            var meetupEvent = _applicationService.Get(eventId);

            return meetupEvent is null
                ? NotFound($"MeetupEvent {eventId} not found")
                : Ok(meetupEvent);
        }

        [HttpPost]
        public IActionResult CreateEvent(string group, MeetupEvent meetupEvent)
        {
            if (meetupEvent.Capacity == 0)
                meetupEvent = meetupEvent with { Capacity = _options.DefaultCapacity };

            var result = _applicationService.Add(group, meetupEvent);

            return Ok(result);
        }

        [HttpPut("{eventId:guid}/capacity/increase")]
        public IActionResult IncreaseCapacity(Guid eventId, int capacity)
        {
            _applicationService.IncreaseCapacity(new IncreaseCapacity(eventId, capacity));
            return Ok();
        }

        [HttpPut("{eventId:guid}/capacity/reduce")]
        public IActionResult ReduceCapacity(Guid eventId, int capacity)
        {
            _applicationService.ReduceCapacity(new ReduceCapacity(eventId, capacity));
            return Ok();
        }

        [HttpPut("{eventId:guid}")]
        public IActionResult PublishEvent(Guid eventId)
        {
            _applicationService.Publish(eventId);
            return Ok();
        }

        [HttpDelete("{eventId:guid}")]
        public IActionResult CancelEvent(Guid eventId)
        {
            _applicationService.Remove(eventId);
            return Ok();
        }

        [HttpPut("{eventId:guid}/invitations/accept")]
        public IActionResult Accept(Guid eventId, Guid userId)
        {
            // validation of request vs domain validation
            var accepted = _applicationService.AcceptInvitation(new AcceptInvitation(eventId, userId));
            return accepted ? Ok() : NotFound("No capacity available");
        }

        [HttpPut("{eventId:guid}/invitations/decline")]
        public IActionResult Decline(Guid eventId, Guid userId)
        {
            _applicationService.DeclineInvitation(new DeclineInvitation(eventId, userId));
            return Ok();
        }
    }

    public record MeetupEvent([Required] string Title, int Capacity = 100, bool Published = false);

    public record MeetupEventsOptions
    {
        public int DefaultCapacity { get; init; }
    }
}