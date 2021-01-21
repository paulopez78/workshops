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
                
            // user profile client
                // get email and name
        }

        public Task Handle(object command)
        {
            switch (command)
            {
                case Commands.V1.NotifyGroupCreated groupCreated:
                {
                    // get all members for group
                   // save notifications 
                   break;
                }
            }
            
            return Task.CompletedTask;
        }
    }
}