using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Meetup.GroupManagement.Contracts;

namespace Meetup.GroupManagement
{
    public class MeetupGroupManagementService : MeetupGroupManagement.MeetupGroupManagementBase
    {
        readonly ILogger<MeetupGroupManagementService> Logger;
        readonly IMediator                             Mediator;

        public MeetupGroupManagementService(IMediator mediator, ILogger<MeetupGroupManagementService> logger)
        {
            Logger   = logger;
            Mediator = mediator;
        }

        public override async Task<CommandReply> Create(CreateRequest command, ServerCallContext context)
        {
            var groupId     = ParseGuid(command.Id, "GroupId");
            var organizerId = ParseGuid(command.OrganizerId, nameof(command.OrganizerId));

            var result = await Mediator.Send(
                new Application.CreateRequest(groupId, organizerId, command.Slug,
                    command.Title, command.Description, command.Location)
            );

            return new() {GroupId = result.GroupId.ToString()};
        }

        public override async Task<CommandReply> UpdateDetails(UpdateDetailsRequest command, ServerCallContext context)
        {
            var groupId = ParseGuid(command.Id, "GroupId");

            var result = await Mediator.Send(
                new Application.UpdateGroupDetailsRequest(groupId, command.Title, command.Description, command.Location)
            );

            return new() {GroupId = result.GroupId.ToString()};
        }

        public override async Task<CommandReply> Join(JoinRequest command, ServerCallContext context)
        {
            var groupId = ParseGuid(command.GroupId, nameof(command.GroupId));
            var userId  = ParseGuid(command.UserId, nameof(command.UserId));

            var result = await Mediator.Send(
                new Application.JoinRequest(groupId, userId)
            );

            return new() {GroupId = result.GroupId.ToString()};
        }

        public override async Task<CommandReply> Leave(LeaveRequest command, ServerCallContext context)
        {
            var groupId = ParseGuid(command.GroupId, nameof(command.GroupId));
            var userId  = ParseGuid(command.UserId, nameof(command.UserId));

            var result = await Mediator.Send(
                new Application.LeaveRequest(groupId, userId, command.Reason)
            );

            return new() {GroupId = result.GroupId.ToString()};
        }

        public override async Task<GetGroup.Types.GetGroupReply> Get(GetGroup query, ServerCallContext context)
        {
            var result = query.IdCase switch
            {
                GetGroup.IdOneofCase.GroupId
                    => await Mediator.Send(new Queries.GetGroupById(ParseGuid(query.GroupId, nameof(query.GroupId)))),
                GetGroup.IdOneofCase.GroupSlug
                    => await Mediator.Send(new Queries.GetGroupBySlug(query.GroupSlug)),
                _
                    => throw new ArgumentException(nameof(query.IdCase)),
            };

            return new()
            {
                Group = new GetGroup.Types.Group()
                {
                    Id          = result.Id.ToString(),
                    Slug        = result.Slug,
                    OrganizerId = result.OrganizerId.ToString(),
                    Title       = result.Title,
                    Description = result.Description,
                    Members =
                    {
                        result.Members.Select(x => new GetGroup.Types.Member()
                        {
                            UserId   = x.UserId.ToString(),
                            JoinedAt = new DateTimeOffset(x.JoinedAt).ToTimestamp()
                        })
                    }
                }
            };
        }

        static Guid ParseGuid(string id, string parameterName)
        {
            if (!Guid.TryParse(id, out var parsed))
                throw new ArgumentException($"Invalid {parameterName}:{id}");

            return parsed;
        }
    }
}