using System.Threading.Tasks;
using Meetup.Scheduling.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using static Meetup.Scheduling.Application.AttendantList.Commands.V1;

namespace Meetup.Scheduling.Application.AttendantList
{
    public class AttendantListCommandApi : MeetupController 
    {
        readonly AttendantListApplicationService ApplicationService;

        public AttendantListCommandApi(AttendantListApplicationService applicationService) => ApplicationService = applicationService;

        [HttpPost("attendants")]
        public Task<IActionResult> Post(CreateAttendantList command) =>
            Handle(ApplicationService, command);

        [HttpPut("attendants/capacity/increase")]
        public Task<IActionResult> IncreaseCapacity(IncreaseCapacity command) =>
            Handle(ApplicationService, command);

        [HttpPut("attendants/capacity/reduce")]
        public Task<IActionResult> ReduceCapacity(ReduceCapacity command) =>
            Handle(ApplicationService, command);

        [HttpPut("attendants/accept")]
        public Task<IActionResult> Accept(AcceptInvitation command) =>
            Handle(ApplicationService, command);

        [HttpPut("attendants/decline")]
        public Task<IActionResult> Decline(DeclineInvitation command) =>
            Handle(ApplicationService, command);
    }
}