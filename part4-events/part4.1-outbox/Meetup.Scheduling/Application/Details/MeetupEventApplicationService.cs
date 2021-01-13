using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.Application.Details.Commands.V1;
using static System.Guid;

namespace Meetup.Scheduling.Application.Details
{
    public class MeetupEventDetailsApplicationService : IApplicationService
    {
        readonly MeetupRepository<MeetupEventDetailsAggregate> MeetupEventRepository;
        readonly IDateTimeProvider                             DateTimeProvider;

        public MeetupEventDetailsApplicationService(
            MeetupRepository<MeetupEventDetailsAggregate> meetupEventRepository,
            IDateTimeProvider dateTimeProvider)
        {
            MeetupEventRepository = meetupEventRepository;
            DateTimeProvider      = dateTimeProvider;
        }

        public Task<CommandResult> Handle(object command)
        {
            return command switch
            {
                Create cmd
                    => HandleCreate(cmd),
                UpdateDetails cmd
                    => Handle(
                        cmd.EventId,
                        entity => entity.UpdateDetails(Domain.Details.From(cmd.Title, cmd.Description))
                    ),
                MakeOnline cmd
                    => Handle(
                        cmd.EventId,
                        entity => entity.MakeOnlineEvent(new Uri(cmd.Url))
                    ),
                MakeOnsite cmd
                    => Handle(
                        cmd.EventId,
                        entity => entity.MakeOnSiteEvent(Address.From(cmd.Address))
                    ),
                Schedule cmd
                    => Handle(
                        cmd.EventId,
                        entity => entity.Schedule(ScheduleDateTime.From(DateTimeProvider.UtcNow(), cmd.StartTime,
                            cmd.EndTime))
                    ),
                Publish cmd
                    => Handle(
                        cmd.EventId,
                        meetup => meetup.Publish()
                    ),
                Cancel cmd
                    => Handle(
                        cmd.EventId,
                        meetup => meetup.Cancel(cmd.Reason)
                    ),
                _
                    => throw new ApplicationException("command handler not found")
            };
        }

        async Task<CommandResult> HandleCreate(Create cmd)
        {
            var meetupEventId = NewGuid();

            var meetupDetails = new MeetupEventDetailsAggregate(
                meetupEventId,
                GroupSlug.From(cmd.Group),
                Domain.Details.From(cmd.Title, cmd.Description),
                cmd.Capacity
            );

            await MeetupEventRepository.Save(meetupDetails);

            return new(meetupEventId);
        }

        async Task<CommandResult> Handle(Guid id, Action<MeetupEventDetailsAggregate> action)
        {
            var meetupEventAggregate = await MeetupEventRepository.Load(id);
            if (meetupEventAggregate is null) throw new ApplicationException($"Entity not found {id}");

            action(meetupEventAggregate);

            await MeetupEventRepository.Save(meetupEventAggregate);

            return new(id);
        }
    }
}