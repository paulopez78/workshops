using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MeetupEvents.Infrastructure
{
    public record CommandResult (Guid Id, bool OkResult);

    public interface IApplicationService
    {
        Task<CommandResult> HandleCommand(Guid id, object command);
    }

    public static class ApplicationServiceExtensions
    {
        public static async Task<IActionResult> HandleHttpCommand(
            this IApplicationService appService,
            Guid id,
            object command)
        {
            try
            {
                var (_, ok) = await appService
                    .HandleCommand(
                        id,
                        command
                    );

                return ok
                    ? new OkObjectResult(id)
                    : new NotFoundObjectResult($"Meetup event {id} not found");
            }
            catch (InvalidOperationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }
    }
}