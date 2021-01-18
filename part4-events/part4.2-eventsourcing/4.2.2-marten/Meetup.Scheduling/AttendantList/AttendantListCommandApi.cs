using System;
using System.Threading.Tasks;
using Marten;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.AttendantList.Commands.V1;

namespace Meetup.Scheduling.AttendantList
{
    [Route("/api/meetup/attendants")]
    [ApiController]
    public class AttendantListCommandApi : ControllerBase
    {
        readonly IApplicationService ApplicationService;

        public AttendantListCommandApi(AttendantListApplicationService applicationService, IDocumentStore eventStore, IPublishEndpoint publishEndpoint, UtcNow getUtcNow) 
            => ApplicationService = applicationService.Build(eventStore, publishEndpoint, getUtcNow);

        [HttpPost()]
        public Task<IActionResult> Post(CreateAttendantList command) =>
            HandleCommand(command.MeetupEventId, command);

        [HttpPut("capacity/increase")]
        public Task<IActionResult> IncreaseCapacity(IncreaseCapacity command) =>
            HandleCommand(command.MeetupEventId, command);

        [HttpPut("capacity/reduce")]
        public Task<IActionResult> ReduceCapacity(ReduceCapacity command) =>
            HandleCommand(command.MeetupEventId, command);

        [HttpPut("add")]
        public Task<IActionResult> Attend(Attend command) =>
            HandleCommand(command.MeetupEventId, command);

        [HttpPut("remove")]
        public Task<IActionResult> DontAttend(DontAttend command) =>
            HandleCommand(command.MeetupEventId, command);


        Task<IActionResult> HandleCommand(Guid aggregateId, object command) =>
            ApplicationService.HandleCommand(aggregateId, command, HttpContext.Request.Headers);
    }
}