using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using MeetupEvents.Infrastructure;
using Microsoft.Extensions.Options;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application
{
    public class AttendantListApplicationService : ApplicationService<AttendantListAggregate>
    {
        readonly MeetupEventOptions Options;
        readonly IDateTimeProvider  DateTimeProvider;
        readonly GetMapId           GetMapId;

        public AttendantListApplicationService(
            AttendantListRepository repository,
            IOptions<MeetupEventOptions> options,
            IDateTimeProvider dateTimeProvider,
            GetMapId getMapId) : base(repository)
        {
            Options          = options.Value;
            DateTimeProvider = dateTimeProvider;
            GetMapId         = getMapId;
        }

        public override Task<CommandResult> HandleCommand(Guid id, object command)
        {
            return command switch
            {
                CreateAttendantList cmd
                    => HandleCreate(
                        id,
                        aggregate => aggregate.Create(id, cmd.MeetupEventId, cmd.Capacity, Options.DefaultCapacity)
                    ),

                Open _
                    => HandleWithMapping(
                        id,
                        aggregate => aggregate.Open(DateTimeProvider.GetUtcNow())
                    ),

                Close _
                    => HandleWithMapping(
                        id,
                        entity => entity.Close(DateTimeProvider.GetUtcNow())
                    ),

                IncreaseCapacity cmd
                    => HandleWithMapping(
                        id,
                        entity => entity.IncreaseCapacity(cmd.byNumber)
                    ),

                ReduceCapacity cmd
                    => HandleWithMapping(
                        id,
                        entity => entity.ReduceCapacity(cmd.byNumber)
                    ),

                Attend cmd
                    => HandleWithMapping(
                        id,
                        entity => entity.Attend(cmd.MemberId, DateTimeProvider.GetUtcNow())
                    ),

                CancelAttendance cmd
                    => HandleWithMapping(
                        id,
                        entity => entity.CancelAttendance(cmd.MemberId, DateTimeProvider.GetUtcNow())
                    ),
                _
                    => throw new InvalidOperationException($"Command handler for {command} does not exist")
            };
        }

        async Task<CommandResult> HandleWithMapping(Guid id, Action<AttendantListAggregate> commandHandler)
        {
            var mapId = await GetMapId(id);
            if (!mapId.HasValue)
                throw new ArgumentException($"Can not map {id}");

            return await Handle(mapId.Value, commandHandler);
        }
    }

    public record MeetupEventOptions()
    {
        public int DefaultCapacity { get; init; } = 100;
    }
}