using System;
using System.Threading.Tasks;
using Marten;
using MassTransit;
using Meetup.Scheduling.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static Meetup.Scheduling.MeetupDetails.Commands.V1;

namespace Meetup.Scheduling.MeetupDetails
{
    [Route("/api/meetup/events")]
    [ApiController]
    public class MeetupEventDetailsCommandApi : ControllerBase
    {
        readonly IApplicationService ApplicationService;

        public MeetupEventDetailsCommandApi(
            MeetupEventDetailsApplicationService applicationService,
            IDocumentStore eventStore,
            IPublishEndpoint publishEndpoint, UtcNow getUtcNow
        )
            => ApplicationService = applicationService.Build(eventStore, publishEndpoint, getUtcNow);

        [HttpPost("details")]
        public Task<IActionResult> Post(Create command) =>
            HandleCommand(command.EventId, command);

        [HttpPut("details")]
        public Task<IActionResult> UpdateDetails(UpdateDetails command) =>
            HandleCommand(command.EventId, command);

        [HttpPut("schedule")]
        public Task<IActionResult> Schedule(Schedule command) =>
            HandleCommand(command.EventId, command);

        [HttpPut("makeonline")]
        public Task<IActionResult> MakeOnline(MakeOnline command) =>
            HandleCommand(command.EventId, command);

        [HttpPut("publish")]
        public Task<IActionResult> PublishEvent(Publish command) =>
            HandleCommand(command.EventId, command);

        [HttpPut("cancel")]
        public Task<IActionResult> CancelEvent(Cancel command) =>
            HandleCommand(command.EventId, command);


        Task<IActionResult> HandleCommand(Guid aggregateId, object command) =>
            ApplicationService.HandleCommand(aggregateId, command, HttpContext.Request.Headers);
    }
}