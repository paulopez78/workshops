using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Meetup.Scheduling.Application.AttendantList.Commands.V1;

namespace Meetup.Scheduling.Application.AttendantList
{
    [Route("/api/meetup/attendants")]
    [ApiController]
    public class AttendantListCommandApi : ControllerBase
    {
        readonly IApplicationService ApplicationService;

        public AttendantListCommandApi(AttendantListApplicationService applicationService,
            ILogger<AttendantListCommandApi> logger)
            => ApplicationService = applicationService.Build(logger);

        [HttpPost()]
        public Task<IActionResult> Post(CreateAttendantList command) =>
            ApplicationService.HandleCommand(command);

        [HttpPut("capacity/increase")]
        public Task<IActionResult> IncreaseCapacity(IncreaseCapacity command) =>
            ApplicationService.HandleCommand(command);

        [HttpPut("capacity/reduce")]
        public Task<IActionResult> ReduceCapacity(ReduceCapacity command) =>
            ApplicationService.HandleCommand(command);

        [HttpPut("accept")]
        public Task<IActionResult> Accept(AcceptInvitation command) =>
            ApplicationService.HandleCommand(command);

        [HttpPut("decline")]
        public Task<IActionResult> Decline(DeclineInvitation command) =>
            ApplicationService.HandleCommand(command);
    }
}