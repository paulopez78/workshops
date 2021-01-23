using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static System.Guid;

namespace Meetup.EndToEndTest
{
    public class EndToEndTest: IClassFixture<ClientsFixture>
    {
        public EndToEndTest(ClientsFixture fixture)
        {
            Fixture = fixture;
        }

        private readonly ClientsFixture Fixture;


        private string NetCoreBcn = "netcorebcn";

        [Fact]
        public async Task Meetup_EndToEnd_Test()
        {
            // Create organizer user
            await CreateUserProfile(Pau);

            // Create some users
            await CreateUserProfile(Joe, Carla, Alice, Bob);

            // Start meetup group (organizer is added as member)
            StartMeetupGroup(NetCoreBcn, organizer: Pau);

            // Add some members to the group
            AddGroupMember(NetCoreBcn, Joe, Carla, Alice, Bob);

            // create meetup
            CreateMeetupEvent(NetCoreBcn, capacity: 2);

            // publish meetup -> side effect notifications created
            var meetupId = PublishMeetupEvent(NetCoreBcn);
            // Wait for attendant list opened and notifications sent to all group members

            Attend(meetupId, Joe);
            Attend(meetupId, Carla);
            Attend(meetupId, Alice);
            Attend(meetupId, Bob);
            // Wait for notifications

            DontAttend(meetupId, Joe);
            // Wait for notifications, check attendant list

            ReduceCapacity(meetupId, 1);
            IncreaseCapacity(meetupId, 1);

            LeaveGroup(NetCoreBcn, Alice);
            // Wait for notifications, check attendant list

            // check meetup status is finished, attendant list is archived 
        }

        private void IncreaseCapacity(Guid meetupId, int byNumber)
        {
            throw new NotImplementedException();
        }

        private void ReduceCapacity(Guid meetupId, int byNumber)
        {
            throw new NotImplementedException();
        }

        private void LeaveGroup(string netCoreBcn, User alice)
        {
            throw new NotImplementedException();
        }

        private void DontAttend(Guid meetupId, User joe)
        {
            throw new NotImplementedException();
        }

        private void Attend(Guid meetupId, User bob)
        {
            throw new NotImplementedException();
        }

        private Guid PublishMeetupEvent(string netCoreBcn)
        {
            throw new NotImplementedException();
        }

        private void CreateMeetupEvent(string @group, int capacity)
        {
            // create meetup and schedule
            // start in 30 seconds stop in 1 minute to test schedule
        }

        private void AddGroupMember(string group, params User[] user)
        {
            throw new NotImplementedException();
        }

        private void StartMeetupGroup(string groupSlug, User organizer)
        {
            throw new NotImplementedException();
        }

        private async Task CreateUserProfile(params User[] users)
        {
            foreach (var user in users)
            {
                await Fixture.UserProfile.CreateOrUpdateAsync(new()
                    {
                        UserId    = user.Id.ToString(),
                        FirstName = user.Name,
                        LastName  = user.Lastname,
                        Email     = user.Email,
                    }
                );
            }
        }

        User Pau   = new(NewGuid(), "Pau", "Lopez", "pau.lopez@meetup.com");
        User Joe   = new(NewGuid(), "Joe", "Smith", "joe.smith@meetup.com");
        User Carla = new(NewGuid(), "Carla", "Garcia", "carla.garcia@meetup.com");
        User Alice = new(NewGuid(), "Alice", "Joplin", "alice.joplin@meetup.com");
        User Bob   = new(NewGuid(), "Bob", "Dylan", "bob.dylan@meetup.com");

        public record User (Guid Id, string Name, string Lastname, string Email);
    }
}