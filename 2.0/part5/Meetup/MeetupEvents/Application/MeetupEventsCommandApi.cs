using System.Threading.Tasks;
using MeetupEvents.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static MeetupEvents.Contracts.MeetupEventsCommands.V1;

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
        public Task<IActionResult> Create(CreateMeetupEvent command)
            => AppService.HandleHttpCommand(command.Id, command);
        
        [HttpPut("details")]
        public Task<IActionResult> UpdateDetails(UpdateDetails command)
            => AppService.HandleHttpCommand(command.Id, command);

        [HttpPut("publish")]
        public Task<IActionResult> Publish(Publish command)
            => AppService.HandleHttpCommand(command.Id, command);

        [HttpPut("cancel")]
        public Task<IActionResult> Cancel(Cancel command)
            => AppService.HandleHttpCommand(command.Id, command);
    }
}