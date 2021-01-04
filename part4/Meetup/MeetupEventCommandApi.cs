using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Meetup.Scheduling.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static Meetup.Scheduling.Commands.V1;

namespace Meetup.Scheduling
{
    [Route("/api/meetup/{group}/events")]
    [ApiController]
    public class MeetupEventCommandApi : ControllerBase
    {
        readonly MeetupEventApplicationService ApplicationService;
        readonly MeetupEventsOptions Options;

        public MeetupEventCommandApi(
            MeetupEventApplicationService applicationService,
            IOptions<MeetupEventsOptions> options)
        {
            ApplicationService = applicationService;
            Options = options.Value;
        }

        [HttpPost]
        public Task<IActionResult> Post(string group, MeetupEvent meetupEvent) =>
            Handle(
                new Create(
                    @group,
                    meetupEvent.Title,
                    meetupEvent.Capacity == 0 ? Options.DefaultCapacity : meetupEvent.Capacity)
            );

        [HttpPut("{eventId:guid}/capacity/increase")]
        public Task<IActionResult> IncreaseCapacity(Guid eventId, int capacity) =>
            Handle(new IncreaseCapacity(eventId, capacity));

        [HttpPut("{eventId:guid}/capacity/reduce")]
        public Task<IActionResult> ReduceCapacity(Guid eventId, int capacity) =>
            Handle(new ReduceCapacity(eventId, capacity));

        [HttpPut("{eventId:guid}/publish")]
        public Task<IActionResult> PublishEvent(Guid eventId) =>
            Handle(new Publish(eventId));

        [HttpPut("{eventId:guid}/cancel")]
        public Task<IActionResult> CancelEvent(Guid eventId) =>
            Handle(new Cancel(eventId));

        [HttpPut("{eventId:guid}/invitations/accept")]
        public Task<IActionResult> Accept(Guid eventId, Guid userId) =>
            Handle(new AcceptInvitation(eventId, userId));

        [HttpPut("{eventId:guid}/invitations/decline")]
        public Task<IActionResult> Decline(Guid eventId, Guid userId) =>
            Handle(new DeclineInvitation(eventId, userId));

        async Task<IActionResult> Handle(object command)
        {
            try
            {
                var result = await ApplicationService.Handle(command);
                return Ok(result);
            }
            catch (ApplicationException e)
            {
                return BadRequest(e.Message);
            }
        }
    }

    public record MeetupEvent([Required] string Title, int Capacity = 100);

    public record MeetupEventsOptions
    {
        public int DefaultCapacity { get; init; }
    }
}