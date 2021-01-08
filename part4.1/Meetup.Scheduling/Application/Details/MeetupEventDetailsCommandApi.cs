using System.Threading.Tasks;
using Meetup.Scheduling.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Meetup.Scheduling.Application.Details.Commands.V1;

namespace Meetup.Scheduling.Application.Details
{
    public class MeetupEventDetailsCommandApi : MeetupController 
    {
        readonly MeetupEventDetailsApplicationService ApplicationService;

        public MeetupEventDetailsCommandApi(MeetupEventDetailsApplicationService applicationService) => ApplicationService = applicationService;

        [HttpPost("events/details")]
        public Task<IActionResult> Post(Create command) =>
            Handle(ApplicationService, command);

        [HttpPut("events/details")]
        public Task<IActionResult> UpdateDetails(UpdateDetails command) =>
            Handle(ApplicationService, command);

        [HttpPut("events/publish")]
        public Task<IActionResult> PublishEvent(Publish command) =>
            Handle(ApplicationService, command);

        [HttpPut("events/cancel")]
        public Task<IActionResult> CancelEvent(Cancel command) =>
            Handle(ApplicationService, command);
    }
}