using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Framework;
using Microsoft.AspNetCore.Mvc;
using static Meetup.Scheduling.Contracts.MeetupDetailsCommands.V1;

namespace Meetup.Scheduling.MeetupDetails
{
    [Route("/api/meetup/events")]
    [ApiController]
    public class MeetupDetailsCommandApi : ControllerBase
    {
        readonly HandleCommand<MeetupDetailsAggregate> HandleCommand;

        public MeetupDetailsCommandApi(HandleCommand<MeetupDetailsAggregate> handle)
            => HandleCommand = handle;

        [HttpPost("details")]
        public Task<IActionResult> Post(CreateMeetup command) =>
            Handle(command.EventId, command);

        [HttpPut("details")]
        public Task<IActionResult> UpdateDetails(UpdateDetails command) =>
            Handle(command.EventId, command);

        [HttpPut("schedule")]
        public Task<IActionResult> Schedule(Schedule command) =>
            Handle(command.EventId, command);

        [HttpPut("makeonline")]
        public Task<IActionResult> MakeOnline(MakeOnline command) =>
            Handle(command.EventId, command);

        [HttpPut("publish")]
        public Task<IActionResult> PublishEvent(Publish command) =>
            Handle(command.EventId, command);

        [HttpPut("cancel")]
        public Task<IActionResult> CancelEvent(Cancel command) =>
            Handle(command.EventId, command);
        
        [HttpPut("start")]
        public Task<IActionResult> StartEvent(Start command) =>
            Handle(command.EventId, command);

        [HttpPut("finish")]
        public Task<IActionResult> FinishEvent(Finish command) =>
            Handle(command.EventId, command);
        
        Task<IActionResult> Handle(Guid id, object command)
            => HandleCommand.WithContext(HttpContext.Request.Headers)(id, command);
    }
}