using System.Threading.Tasks;
using MeetupEvents.Contracts;
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
        public Task<IActionResult> Create(Commands.V1.Create command)
            => AppService.HandleHttpCommand(command.Id, command);
        
        [HttpPut("details")]
        public Task<IActionResult> UpdateDetails(Commands.V1.UpdateDetails command)
            => AppService.HandleHttpCommand(command.Id, command);

        [HttpPut("publish")]
        public Task<IActionResult> Publish(Commands.V1.Publish command)
            => AppService.HandleHttpCommand(command.Id, command);

        [HttpPut("cancel")]
        public Task<IActionResult> Cancel(Commands.V1.Cancel command)
            => AppService.HandleHttpCommand(command.Id, command);
        
        [HttpPut("attend")]
        public Task<IActionResult> Attend(Commands.V1.Attend command)
            => AppService.HandleHttpCommand(command.Id, command);
        
        [HttpPut("cancel-attendance")]
        public Task<IActionResult> CancelAttendance(Commands.V1.CancelAttendance command)
            => AppService.HandleHttpCommand(command.Id, command);
    }
}