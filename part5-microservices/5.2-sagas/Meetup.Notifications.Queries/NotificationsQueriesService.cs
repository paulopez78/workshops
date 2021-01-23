using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using MongoDB.Driver;
using Meetup.Notifications.Queries.Contracts.V1;
using MongoDB.Driver.Linq;
using static Meetup.Notifications.Contracts.ReadModels.V1;

namespace Meetup.Notifications.Queries
{
    public class NotificationsQueriesService : NotificationsQueries.NotificationsQueriesBase
    {
        readonly IMongoCollection<Notification> DbCollection;

        public NotificationsQueriesService(IMongoDatabase database)
        {
            DbCollection = database.GetCollection<Notification>(nameof(Notification));
        }

        public override async Task<GetNotificationRequest.Types.GeNotificationReply> Get(GetNotificationRequest request,
            ServerCallContext context)
        {
            var notifications = await DbCollection.AsQueryable().Where(x => x.UserId == request.UserId).ToListAsync();

            return new()
            {
                Notifications =
                {
                    notifications.Select(x => new GetNotificationRequest.Types.Notification()
                    {
                        NotificationId   = x.Id,
                        NotificationType = x.NotificationType.ToString(),
                        GroupId          = x.GroupId,
                        MeetupId         = x.MeetupId,
                        MemberId         = x.MemberId,
                        Message          = x.Message
                    })
                }
            };
        }
    }
}