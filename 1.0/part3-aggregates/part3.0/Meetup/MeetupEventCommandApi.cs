using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Meetup.Scheduling.Commands.V1;

namespace Meetup.Scheduling
{
    [Route("/api/meetup/{group}/events")]
    [ApiController]
    public class MeetupEventCommandApi : ControllerBase
    {
        readonly MeetupEventApplicationService ApplicationService;
        readonly MeetupEventsOptions           Options;

        public MeetupEventCommandApi(
            MeetupEventApplicationService applicationService,
            IOptions<MeetupEventsOptions> options)
        {
            ApplicationService = applicationService;
            Options            = options.Value;
        }

        [HttpPost]
        public Task<IActionResult> Post(Create command) =>
            Handle(command.Capacity == 0 ? command with {Capacity = Options.DefaultCapacity} : command);

        [HttpPut("{eventId:guid}/details")]
        public Task<IActionResult> UpdateDetails(UpdateDetails command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/capacity/increase")]
        public Task<IActionResult> IncreaseCapacity(IncreaseCapacity command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/capacity/reduce")]
        public Task<IActionResult> ReduceCapacity(ReduceCapacity command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/publish")]
        public Task<IActionResult> PublishEvent(Publish command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/cancel")]
        public Task<IActionResult> CancelEvent(Cancel command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/invitations/accept")]
        public Task<IActionResult> Accept(AcceptInvitation command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/invitations/decline")]
        public Task<IActionResult> Decline(DeclineInvitation command) =>
            Handle(command);

        async Task<IActionResult> Handle(object command)
        {
            try
            {
                var result = await ApplicationService.Handle(command);
                return Ok(new CommandResult(result));
            }
            catch (ApplicationException e)
            {
                return BadRequest(e.Message);
            }

            catch (DbUpdateConcurrencyException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }

    public record CommandResult(Guid EventId);

    public record MeetupEventsOptions
    {
        public int DefaultCapacity { get; init; }
    }
}