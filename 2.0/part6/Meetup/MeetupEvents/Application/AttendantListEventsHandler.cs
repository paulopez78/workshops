using System;
using System.Threading.Tasks;
using MassTransit;
using static MeetupEvents.Contracts.MeetupEvents.V1;
using static MeetupEvents.Contracts.AttendantListCommands.V1;

namespace MeetupEvents.Application
{
    public class AttendantListEventsHandler :
        IConsumer<MeetupEventCreated>,
        IConsumer<Published>,
        IConsumer<Canceled>
    {
        readonly AttendantListApplicationService ApplicationService;

        public AttendantListEventsHandler(AttendantListApplicationService appService) =>
            ApplicationService = appService;

        public Task Consume(ConsumeContext<MeetupEventCreated> context)
        {
            var attendantListId = Guid.NewGuid();

            return ApplicationService.HandleCommand(
                attendantListId,
                new CreateAttendantList(attendantListId, context.Message.Id, 0)
            );
        }

        public Task Consume(ConsumeContext<Published> context) =>
            ApplicationService.HandleCommand(context.Message.Id, new Open(context.Message.Id));

        public Task Consume(ConsumeContext<Canceled> context) =>
            ApplicationService.HandleCommand(context.Message.Id, new Close(context.Message.Id));
    }
}