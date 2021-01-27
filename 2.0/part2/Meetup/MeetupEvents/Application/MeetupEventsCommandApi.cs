using System;
using System.Threading.Tasks;
using MeetupEvents.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Application
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventsCommandApi : ControllerBase
    {
        readonly IApplicationService AppService;

        public MeetupEventsCommandApi(
            MeetupEventsApplicationService applicationService,
            ILogger<MeetupEventsCommandApi> logger) =>
            AppService = new ExceptionLoggingMiddleware<MeetupEventsCommandApi>(applicationService, logger);

        [HttpPost]
        public Task<IActionResult> Create(Create command)
            => AppService.HandleHttpCommand(command.Id, command);

        [HttpPut("publish")]
        public Task<IActionResult> Publish(Publish command)
            => AppService.HandleHttpCommand(command.Id, command);

        [HttpPut("cancel")]
        public Task<IActionResult> Cancel(Cancel command)
            => AppService.HandleHttpCommand(command.Id, command);
    }

    public record Create(Guid Id, string Title, int Capacity);

    public record Publish(Guid Id);

    public record Cancel(Guid Id, string Reason);
}