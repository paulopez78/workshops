using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Meetup.GroupManagement.Data;

namespace Meetup.GroupManagement.Application
{
    public class NotificationsHandler : INotificationHandler<GroupCreated>, INotificationHandler<MemberJoined>,
        INotificationHandler<MemberLeft>
    {
        readonly MeetupGroupManagementDbContext DbContext;
        readonly IPublishEndpoint               PublishEndpoint;

        public NotificationsHandler(IPublishEndpoint publishEndpoint, MeetupGroupManagementDbContext dbContext)
        {
            DbContext       = dbContext;
            PublishEndpoint = publishEndpoint;
        }

        public async Task Handle(GroupCreated notification, CancellationToken cancellationToken)
        {
            // translate to integration(external) event
            await PublishEndpoint.Publish(notification, cancellationToken);
        }

        public async Task Handle(MemberJoined notification, CancellationToken cancellationToken)
        {
            // translate to integration(external) event
            await PublishEndpoint.Publish(notification, cancellationToken);
        }

        public async Task Handle(MemberLeft notification, CancellationToken cancellationToken)
        {
            // translate to integration(external) event
            await PublishEndpoint.Publish(notification, cancellationToken);
        }
    }
}