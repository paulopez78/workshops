using System.Threading.Tasks;
using Meetup.Scheduling.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Meetup.Scheduling.Application.Details.Commands.V1;

namespace Meetup.Scheduling.Application.Details
{
    [Route("/api/meetup/events")]
    [ApiController]
    public class MeetupEventDetailsCommandApi : ControllerBase
    {
        readonly IApplicationService ApplicationService;

        public MeetupEventDetailsCommandApi(MeetupEventDetailsApplicationService applicationService, ILogger<MeetupEventDetailsCommandApi> logger)
            => ApplicationService = applicationService.Build(logger);

        [HttpPost("details")]
        public Task<IActionResult> Post(Create command) =>
            ApplicationService.HandleCommand(command);

        [HttpPut("details")]
        public Task<IActionResult> UpdateDetails(UpdateDetails command) =>
            ApplicationService.HandleCommand(command);
        
        [HttpPut("schedule")]
        public Task<IActionResult> Schedule(Schedule command) =>
            ApplicationService.HandleCommand(command);
        
        [HttpPut("makeonline")]
        public Task<IActionResult> MakeOnline(MakeOnline command) =>
            ApplicationService.HandleCommand(command);

        [HttpPut("publish")]
        public Task<IActionResult> PublishEvent(Publish command) =>
            ApplicationService.HandleCommand(command);

        [HttpPut("cancel")]
        public Task<IActionResult> CancelEvent(Cancel command) =>
            ApplicationService.HandleCommand(command);
    }
}