using System;
using System.Threading.Tasks;
using MeetupEvents.Infrastructure;

namespace MeetupEvents.Application
{
    public class MeetupEventsApplicationService : IApplicationService
    {
        readonly MeetupEventsRepository Repository;

        public MeetupEventsApplicationService(MeetupEventsRepository repository)
        {
            Repository = repository;
        }

        public Task<CommandResult> HandleCommand(Guid id, object command)
        {
            return command switch
            {
                Create cmd => HandleCreate(id, () => Create(id,cmd.Title, cmd.Capacity)),
                Publish _  => Handle(id, Publish),
                Cancel cmd => Handle(id, entity => Cancel(entity, cmd.Reason)),
                _          => throw new InvalidOperationException($"Command handler for {command} does not exist")
            };
        }

        public void Publish(MeetupEvent meetup)
        {
            if (meetup.Status == MeetupEventStatus.Cancelled)
                throw new InvalidOperationException($"Can not publish already cancelled meetup {meetup}");

            meetup.Status = MeetupEventStatus.Published;
        }

        public void Cancel(MeetupEvent meetup, string reason)
        {
            if (meetup.Status != MeetupEventStatus.Published)
                throw new InvalidOperationException($"Can not cancel not published meetup {meetup}");

            meetup.Status            = MeetupEventStatus.Cancelled;
            meetup.CancelationReason = reason;
        }

        public MeetupEvent Create(Guid id, string title, int capacity) =>
            new(id, title, capacity)
            {
                Status = MeetupEventStatus.Draft
            };

        async Task<CommandResult> Handle(Guid id, Action<MeetupEvent> commandHandler)
        {
            // load entity
            var meetup = await Repository.Get(id);
            if (meetup is null) return new(id, false);

            // execute business logic
            commandHandler(meetup);

            // commit transaction
            await Repository.SaveChanges();
            return new(id, true);
        }

        async Task<CommandResult> HandleCreate(Guid id, Func<MeetupEvent> commandHandler)
        {
            // check if already exists
            var meetup = await Repository.Get(id);
            if (meetup is not null) throw new InvalidOperationException($"Meetup {id} already exists");

            // execute business logic
            var newMeetup = commandHandler();
            await Repository.Add(newMeetup);

            // commit transaction
            await Repository.SaveChanges();
            return new(id, true);
        }
    }
}