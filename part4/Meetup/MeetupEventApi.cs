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
    public class MeetupEventApi : ControllerBase
    {
        readonly MeetupEventApplicationService ApplicationService;
        readonly MeetupEventPostgresQueries Queries;
        readonly MeetupEventsOptions Options;

        public MeetupEventApi(
            MeetupEventApplicationService applicationService,
            MeetupEventPostgresQueries queries,
            IOptions<MeetupEventsOptions> options)
        {
            ApplicationService = applicationService;
            Queries = queries;
            Options = options.Value;
        }

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

        [HttpPost]
        public async Task<IActionResult> Post(string group, MeetupEvent meetupEvent)
        {
            var result = await ApplicationService.Handle(
                new Create(
                    group,
                    meetupEvent.Title,
                    meetupEvent.Capacity == 0 ? Options.DefaultCapacity : meetupEvent.Capacity)
            );
            return Ok(result);
        }

        [HttpPut("{eventId:guid}/capacity/increase")]
        public async Task<IActionResult> IncreaseCapacity(Guid eventId, int capacity)
        {
            await ApplicationService.Handle(new IncreaseCapacity(eventId, capacity));
            return Ok();
        }

        [HttpPut("{eventId:guid}/capacity/reduce")]
        public async Task<IActionResult> ReduceCapacity(Guid eventId, int capacity)
        {
            await ApplicationService.Handle(new ReduceCapacity(eventId, capacity));
            return Ok();
        }

        [HttpPut("{eventId:guid}")]
        public async Task<IActionResult> PublishEvent(Guid eventId)
        {
            await ApplicationService.Handle(new Publish(eventId));
            return Ok();
        }

        [HttpDelete("{eventId:guid}")]
        public async Task<IActionResult> CancelEvent(Guid eventId)
        {
            await ApplicationService.Handle(new Cancel(eventId));
            return Ok();
        }

        [HttpPut("{eventId:guid}/invitations/accept")]
        public async Task<IActionResult> Accept(Guid eventId, Guid userId)
        {
            // validation of request vs domain validation
            await ApplicationService.Handle(new AcceptInvitation(eventId, userId));
            return Ok();
        }

        [HttpPut("{eventId:guid}/invitations/decline")]
        public async Task<IActionResult> Decline(Guid eventId, Guid userId)
        {
            await ApplicationService.Handle(new DeclineInvitation(eventId, userId));
            return Ok();
        }
    }

    public record MeetupEvent([Required] string Title, int Capacity = 100);

    public record MeetupEventsOptions
    {
        public int DefaultCapacity { get; init; }
    }
}