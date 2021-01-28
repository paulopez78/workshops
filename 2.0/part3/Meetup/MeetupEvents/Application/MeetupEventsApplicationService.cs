using System;
using System.Threading.Tasks;
using MeetupEvents.Contracts;
using MeetupEvents.Domain;
using MeetupEvents.Infrastructure;
using Microsoft.Extensions.Options;

namespace MeetupEvents.Application
{
    public class MeetupEventsApplicationService : IApplicationService
    {
        readonly MeetupEventsRepository Repository;
        readonly MeetupEventOptions     Options;
        readonly IDateTimeProvider      DateTimeProvider;

        public MeetupEventsApplicationService(
            MeetupEventsRepository repository,
            IOptions<MeetupEventOptions> options,
            IDateTimeProvider dateTimeProvider)
        {
            Repository       = repository;
            Options          = options.Value;
            DateTimeProvider = dateTimeProvider;
        }

        public Task<CommandResult> HandleCommand(Guid id, object command)
        {
            return command switch
            {
                Commands.V1.Create cmd
                    => HandleCreate(
                        id,
                        entity => entity.Create(id, cmd.GroupId, cmd.Title, cmd.Description, cmd.Capacity, Options.DefaultCapacity)
                    ),
                
                Commands.V1.UpdateDetails cmd
                    => Handle(
                        id,
                        entity => entity.UpdateDetails(cmd.Title, cmd.Description)),

                Commands.V1.Publish _
                    => Handle(
                        id,
                        entity => entity.Publish()
                    ),

                Commands.V1.Cancel cmd
                    => Handle(
                        id,
                        entity => entity.Cancel(cmd.Reason)
                    ),

                Commands.V1.Attend cmd
                    => Handle(
                        id,
                        entity => entity.Attend(cmd.MemberId, DateTimeProvider.GetUtcNow())
                    ),

                Commands.V1.CancelAttendance cmd
                    => Handle(
                        id,
                        entity => entity.CancelAttendance(cmd.MemberId, DateTimeProvider.GetUtcNow())
                    ),
                _
                    => throw new InvalidOperationException($"Command handler for {command} does not exist")
            };
        }


        async Task<CommandResult> Handle(Guid id, Action<MeetupEventAggregate> commandHandler)
        {
            // load entity
            var meetup = await Repository.Get(id);
            if (meetup is null) return new(id, false);

            // execute business logic
            commandHandler(meetup);

            // commit transaction
            await Repository.SaveChanges(meetup);
            return new(id, true);
        }

        async Task<CommandResult> HandleCreate(Guid id, Action<MeetupEventAggregate> commandHandler)
        {
            // check if already exists
            var meetup = await Repository.Get(id);
            if (meetup is not null) throw new InvalidOperationException($"Meetup {id} already exists");

            // execute business logic
            var entity = new MeetupEventAggregate();
            commandHandler(entity);
            await Repository.Add(entity);

            // commit transaction
            await Repository.SaveChanges(entity);
            return new(id, true);
        }
    }
}