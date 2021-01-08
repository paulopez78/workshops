using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meetup.Scheduling.Infrastructure
{
    [Route("/api/meetup/")]
    [ApiController]
    public abstract class MeetupController: ControllerBase
    {
        protected async Task<IActionResult> Handle(IApplicationService applicationService, object command)
        {
            try
            {
                var result = await applicationService.Handle(command);
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

    public interface IApplicationService
    {
        Task<Guid> Handle(object command);
    }

    public record CommandResult(Guid EventId);
}