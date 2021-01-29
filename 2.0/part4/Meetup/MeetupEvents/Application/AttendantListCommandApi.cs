using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application
{
    [ApiController]
    [Route("/api/meetup/events/attendant-list")]
    public class AttendantListCommandApi : ControllerBase
    {
        readonly IApplicationService AppService;

        public AttendantListCommandApi(
            AttendantListApplicationService applicationService,
            ILogger<AttendantListCommandApi> logger) =>
            AppService = new ExceptionLoggingMiddleware<AttendantListCommandApi>(applicationService, logger);

        [HttpPost]
        public Task<IActionResult> Create(CreateAttendantList command)
            => AppService.HandleHttpCommand(command.Id, command);

        [HttpPut("open")]
        public Task<IActionResult> Open(Open command)
            => AppService.HandleHttpCommand(command.Id, command);

        [HttpPut("close")]
        public Task<IActionResult> Close(Close command)
            => AppService.HandleHttpCommand(command.Id, command);
        
        [HttpPut("attend")]
        public Task<IActionResult> Attend(Attend command)
            => AppService.HandleHttpCommand(command.Id, command);
        
        [HttpPut("cancel-attendance")]
        public Task<IActionResult> CancelAttendance(CancelAttendance command)
            => AppService.HandleHttpCommand(command.Id, command);
    }
}