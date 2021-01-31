using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Infrastructure
{
    public static class ApplicationServiceHttpExtensions
    {
        public static async Task<IActionResult> HandleHttpCommand(
            this IApplicationService appService,
            Guid id,
            object command)
        {
            try
            {
                var (_, changes) = await appService
                    .HandleCommand(
                        id,
                        command
                    );

                return changes.Any()
                    ? new OkObjectResult(id)
                    : new NotFoundObjectResult($"Meetup event {id} not found");
            }
            catch (InvalidOperationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (ArgumentException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
            catch (DbUpdateConcurrencyException e)
            {
                return new ObjectResult(e.Message) {StatusCode = StatusCodes.Status500InternalServerError};
            }
        }
    }
}