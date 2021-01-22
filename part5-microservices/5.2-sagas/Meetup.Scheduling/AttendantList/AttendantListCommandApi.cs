using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Framework;
using Microsoft.AspNetCore.Mvc;
using static Meetup.Scheduling.Contracts.AttendantListCommands.V1;

namespace Meetup.Scheduling.AttendantList
{
    [Route("/api/meetup/attendants")]
    [ApiController]
    public class AttendantListCommandApi : ControllerBase
    {
        readonly HandleCommand<AttendantListAggregate> HandleCommand;

        public AttendantListCommandApi(HandleCommand<AttendantListAggregate> handle)
            => HandleCommand = handle;

        [HttpPost()]
        public Task<IActionResult> Post(CreateAttendantList command) =>
            Handle(command.Id, command);
        
        [HttpPut("open")]
        public Task<IActionResult> Open(Open command) =>
            Handle(command.Id, command);
        
        [HttpPut("close")]
        public Task<IActionResult> Close(Close command) =>
            Handle(command.Id, command);

        [HttpPut("capacity/increase")]
        public Task<IActionResult> IncreaseCapacity(IncreaseCapacity command) =>
            Handle(command.Id, command);

        [HttpPut("capacity/reduce")]
        public Task<IActionResult> ReduceCapacity(ReduceCapacity command) =>
            Handle(command.Id, command);

        [HttpPut("add")]
        public Task<IActionResult> Attend(Attend command) =>
            Handle(command.Id, command);

        [HttpPut("remove")]
        public Task<IActionResult> DontAttend(DontAttend command) =>
            Handle(command.Id, command);

        Task<IActionResult> Handle(Guid id, object command)
            => HandleCommand.WithContext(HttpContext.Request.Headers)(id, command);
    }
}