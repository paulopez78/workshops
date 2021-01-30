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

        public AttendantListApplicationService(
            AttendantListRepository repository,
            IOptions<MeetupEventOptions> options,
            IDateTimeProvider dateTimeProvider) : base(repository)
        {
            Options          = options.Value;
            DateTimeProvider = dateTimeProvider;
        }

        public override Task<CommandResult> HandleCommand(Guid id, object command)
        {
            return command switch
            {
                CreateAttendantList cmd
                    => HandleCreate(
                        id,
                        entity => entity.Create(id, cmd.MeetupEventId, cmd.Capacity, Options.DefaultCapacity)
                    ),

                Open _
                    => Handle(
                        id,
                        entity => entity.Open()
                    ),

                Close _
                    => Handle(
                        id,
                        entity => entity.Close()
                    ),

                Attend cmd
                    => Handle(
                        id,
                        entity => entity.Attend(cmd.MemberId, DateTimeProvider.GetUtcNow())
                    ),

                CancelAttendance cmd
                    => Handle(
                        id,
                        entity => entity.CancelAttendance(cmd.MemberId, DateTimeProvider.GetUtcNow())
                    ),
                _
                    => throw new InvalidOperationException($"Command handler for {command} does not exist")
            };
        }
    }

    public record MeetupEventOptions()
    {
        public int DefaultCapacity { get; init; } = 100;
    }
}