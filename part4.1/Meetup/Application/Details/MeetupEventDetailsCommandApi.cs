using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using static Meetup.Scheduling.Application.Details.Commands.V1;

namespace Meetup.Scheduling.Application.Details
{
    public class MeetupEventCommandApi : MeetupController 
    {
        readonly MeetupEventDetailsApplicationService ApplicationService;

        public MeetupEventCommandApi(MeetupEventDetailsApplicationService applicationService)
        {
            ApplicationService = applicationService;
        }

        [HttpPost]
        public Task<IActionResult> Post(Create command) =>
            Handle(ApplicationService, command);

        [HttpPut("{eventId:guid}/details")]
        public Task<IActionResult> UpdateDetails(UpdateDetails command) =>
            Handle(ApplicationService, command);

        [HttpPut("{eventId:guid}/publish")]
        public Task<IActionResult> PublishEvent(Publish command) =>
            Handle(ApplicationService, command);

        [HttpPut("{eventId:guid}/cancel")]
        public Task<IActionResult> CancelEvent(Cancel command) =>
            Handle(ApplicationService, command);
    }
}