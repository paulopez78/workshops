using System;
using System.Threading.Tasks;
using MeetupEvents.Infrastructure;
using static MeetupEvents.Contracts.MeetupEvents.V1;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application
{
    public class AttendantListEventsHandler :
        IDomainEventHandler<MeetupEventCreated>,
        IDomainEventHandler<Published>,
        IDomainEventHandler<Canceled>
    {
        readonly AttendantListApplicationService ApplicationService;
        
        public AttendantListEventsHandler(AttendantListApplicationService appService ) => ApplicationService = appService;

        public Task Handle(MeetupEventCreated @event)
        {
            var attendantListId = Guid.NewGuid();

            return ApplicationService.HandleCommand(
                attendantListId,
                new CreateAttendantList(attendantListId, @event.Id, 0)
            );
        }

        public Task Handle(Published @event) => ApplicationService.HandleCommand(@event.Id, new Open(@event.Id));

        public Task Handle(Canceled @event) => ApplicationService.HandleCommand(@event.Id, new Close(@event.Id));
    }
}