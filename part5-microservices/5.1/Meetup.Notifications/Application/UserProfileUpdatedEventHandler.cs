using System.Threading.Tasks;
using MassTransit;
using Meetup.Notifications.Contracts;

namespace Meetup.Notifications.Application
{
    public class UserProfileUpdatedCommandHandler :
        IConsumer<Commands.V1.Notify>,
        IConsumer<Commands.V1.NotifyGroupCreated>,
        IConsumer<Commands.V1.NotifyMeetupPublished>,
        IConsumer<Commands.V1.NotifyMemberJoined>,
        IConsumer<Commands.V1.NotifyMemberLeft>,
        IConsumer<Commands.V1.NotifyMeetupAttendantGoing>,
        IConsumer<Commands.V1.NotifyMeetupAttendantWaiting>
    {
        readonly NotificationsApplicationService ApplicationService;

        public UserProfileUpdatedCommandHandler(NotificationsApplicationService applicationService)
            => ApplicationService = applicationService;

        public Task Consume(ConsumeContext<Commands.V1.Notify> context)
            => ApplicationService.Handle(context.Message);

        public Task Consume(ConsumeContext<Commands.V1.NotifyGroupCreated> context)
            => ApplicationService.Handle(context.Message);

        public Task Consume(ConsumeContext<Commands.V1.NotifyMeetupPublished> context)
            => ApplicationService.Handle(context.Message);

        public Task Consume(ConsumeContext<Commands.V1.NotifyMemberJoined> context)
            => ApplicationService.Handle(context.Message);

        public Task Consume(ConsumeContext<Commands.V1.NotifyMemberLeft> context)
            => ApplicationService.Handle(context.Message);

        public Task Consume(ConsumeContext<Commands.V1.NotifyMeetupAttendantGoing> context)
            => ApplicationService.Handle(context.Message);

        public Task Consume(ConsumeContext<Commands.V1.NotifyMeetupAttendantWaiting> context)
            => ApplicationService.Handle(context.Message);
    }
}