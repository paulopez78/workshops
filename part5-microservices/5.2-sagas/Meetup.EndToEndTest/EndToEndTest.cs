using System;
using Xunit;

namespace Meetup.EndToEndTest
{
    public class EndToEndTest
    {
        Guid Pau   = Guid.NewGuid();
        Guid Joe   = Guid.NewGuid();
        Guid Carla = Guid.NewGuid();
        Guid Alice = Guid.NewGuid();
        Guid Bob   = Guid.NewGuid();

        private string NetCoreBcn = "netcorebcn";

        [Fact]
        public void Meetup_EndToEnd_Test()
        {
            // Create organizer user
            var organizer = Pau;

            // Create organizer user
            CreateUserProfile(Pau);

            // Create some users
            CreateUserProfile(Joe, Carla, Alice, Bob);

            // Start meetup group (organizer is added as member)
            StartMeetupGroup(NetCoreBcn, Pau);

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
            IncreaseCapacity(meetupId,1);

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

        private void LeaveGroup(string netCoreBcn, Guid alice)
        {
            throw new NotImplementedException();
        }

        private void DontAttend(Guid meetupId, Guid joe)
        {
            throw new NotImplementedException();
        }

        private void Attend(Guid meetupId, Guid bob)
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

        private void AddGroupMember(string group, Guid joe, Guid carla, Guid alice, Guid bob)
        {
            throw new NotImplementedException();
        }

        private void StartMeetupGroup(string groupSlug, Guid organizer)
        {
            throw new NotImplementedException();
        }

        private void CreateUserProfile(params Guid[] userIds)
        {
            throw new NotImplementedException();
        }
    }
}