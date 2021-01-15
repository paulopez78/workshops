using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.Application.Details.Commands.V1;

namespace Meetup.Scheduling.Application.Details
{
    [Route("/api/meetup/events")]
    [ApiController]
    public class MeetupEventDetailsCommandApi : ControllerBase
    {
        readonly IApplicationService ApplicationService;

        public MeetupEventDetailsCommandApi(
            MeetupEventDetailsApplicationService applicationService,
            MeetupSchedulingDbContext dbContext,
            IPublishEndpoint publishEndpoint, UtcNow getUtcNow,
            ILogger<MeetupEventDetailsCommandApi> logger)
            => ApplicationService = applicationService.Build(dbContext, publishEndpoint, getUtcNow, logger);

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