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
        readonly MeetupRepository<AttendantListAggregate>      AttendantListRepository;
        readonly IDateTimeProvider                             DateTimeProvider;

        public MeetupEventDetailsApplicationService(
            MeetupRepository<MeetupEventDetailsAggregate> meetupEventRepository,
            MeetupRepository<AttendantListAggregate> attendantListRepository,
            IDateTimeProvider dateTimeProvider)
        {
            MeetupEventRepository   = meetupEventRepository;
            AttendantListRepository = attendantListRepository;
            DateTimeProvider        = dateTimeProvider;
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
                        (entity, _) => entity.UpdateDetails(Domain.Details.From(cmd.Title, cmd.Description))
                    ),
                MakeOnline cmd
                    => Handle(
                        cmd.EventId,
                        (entity, _) => entity.MakeOnlineEvent(new Uri(cmd.Url))
                    ),
                MakeOnsite cmd
                    => Handle(
                        cmd.EventId,
                        (entity, _) => entity.MakeOnSiteEvent(Address.From(cmd.Address))
                    ),
                Schedule cmd
                    => Handle(
                        cmd.EventId,
                        (entity, _) => entity.Schedule(ScheduleDateTime.From(DateTimeProvider.UtcNow(), cmd.StartTime,
                            cmd.EndTime))
                    ),
                Publish cmd
                    => Handle(
                        cmd.EventId,
                        (meetup, attendantList) =>
                        {
                            meetup.Publish();
                            // attendantList.Open();
                        }),
                Cancel cmd
                    => Handle(
                        cmd.EventId,
                        (meetup, attendantList) =>
                        {
                            meetup.Cancel(cmd.Reason);
                            // attendantList.Close();
                        }),
                _
                    => throw new ApplicationException("command handler not found")
            };
        }

        async Task<CommandResult> HandleCreate(Create cmd)
        {
            // instead of using domain events (before commit transaction) we can use normal flow without indirection
            // its more explicit that we have bigger transaction
            var meetupEventId = NewGuid();

            var attendantList = new AttendantListAggregate(meetupEventId, cmd.Capacity);
            await AttendantListRepository.Save(attendantList);

            var meetupDetails = new MeetupEventDetailsAggregate(
                meetupEventId,
                GroupSlug.From(cmd.Group),
                Domain.Details.From(cmd.Title, cmd.Description)
            );
            await MeetupEventRepository.Save(meetupDetails);

            return new(meetupEventId);
        }

        async Task<CommandResult> Handle(Guid id, Action<MeetupEventDetailsAggregate, AttendantListAggregate> action)
        {
            var meetupEventAggregate = await MeetupEventRepository.Load(id);
            if (meetupEventAggregate is null) throw new ApplicationException($"Entity not found {id}");

            var attendantListAggregate = await AttendantListRepository.Load(id);
            if (attendantListAggregate is null) throw new ApplicationException($"Entity not found {id}");

            action(meetupEventAggregate, attendantListAggregate);

            await MeetupEventRepository.Save(meetupEventAggregate);
            //await AttendantListRepository.Save(attendantListAggregate);

            return new(id);
        }
    }
}