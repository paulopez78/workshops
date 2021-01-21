using System.Threading.Tasks;
using Meetup.Notifications.Contracts;
using Meetup.Notifications.Data;
using MongoDB.Driver;

namespace Meetup.Notifications.Application
{
    public class NotificationsApplicationService
    {
        readonly IMongoCollection<Notification> DbCollection;

        public NotificationsApplicationService(IMongoDatabase database)
        {
            DbCollection = database.GetCollection<Notification>(nameof(Notification));

            // group management client
            // get members by group

            // scheduling client
            // get attendants by meetup
        }

        public Task Handle(object command)
        {
            switch (command)
            {
                case Commands.V1.NotifyGroupCreated groupCreated:
                    // get interested users from recommendations system
                    // save notifications 
                    break;

                case Commands.V1.NotifyMeetupPublished published:
                    // get all members for published meetup group
                    // save notifications (bulk write)
                    break;

                case Commands.V1.NotifyMeetupCancelled cancelled:
                    // get all attendants for cancelled meetup
                    // save notifications (bulk write)
                    break;

                case Commands.V1.NotifyMemberJoined joined:
                    // get organizer from group
                    // save notification
                    break;

                case Commands.V1.NotifyMemberLeft left:
                    // get group information and notify the organizer
                    // save notification
                    break;

                case Commands.V1.NotifyMeetupAttendantGoing going:
                    // get attendant, group and meetup information
                    // save notification
                    break;

                case Commands.V1.NotifyMeetupAttendantWaiting waiting:
                    // get attendant, group and meetup information
                    // save notification
                    break;
            }

            return Task.CompletedTask;
        }
    }
}