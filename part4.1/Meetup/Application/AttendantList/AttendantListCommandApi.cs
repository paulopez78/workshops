using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Meetup.Scheduling.Application.AttendantList.Commands.V1;

namespace Meetup.Scheduling.Application.AttendantList
{
    [Route("/api/meetup/{group}/events")]
    [ApiController]
    public class AttendantListCommandApi : ControllerBase
    {
        readonly AttendantListApplicationService ApplicationService;

        public AttendantListCommandApi(AttendantListApplicationService applicationService)
        {
            ApplicationService = applicationService;
        }

        [HttpPost("{eventId:guid}/attendants")]
        public Task<IActionResult> Post(CreateAttendantList command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/capacity/increase")]
        public Task<IActionResult> IncreaseCapacity(IncreaseCapacity command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/capacity/reduce")]
        public Task<IActionResult> ReduceCapacity(ReduceCapacity command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/attendants/accept")]
        public Task<IActionResult> Accept(AcceptInvitation command) =>
            Handle(command);

        [HttpPut("{eventId:guid}/attendants/decline")]
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
}