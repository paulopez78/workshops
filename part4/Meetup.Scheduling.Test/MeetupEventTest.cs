using System;
using System.Linq;
using Xunit;
using Meetup.Scheduling.Domain;
using static System.Guid;

namespace Meetup.Scheduling.Test
{
    public class MeetupEventEntityTest
    {
        [Fact]
        public void Given_Draft_Event_When_Publish_Then_Event_Scheduled()
        {
            // arrange
            var entity = CreateMeetupEvent();

            // act
            entity.Publish();

            // assert
            Assert.Equal(MeetupEventStatus.Scheduled, entity.Status);
        }

        [Fact]
        public void Given_Scheduled_Event_When_Cancel_Then_Event_Cancelled()
        {
            // arrange
            var entity = CreateMeetupEvent();

            // act
            entity.Cancel();

            // assert
            Assert.Equal(MeetupEventStatus.Cancelled, entity.Status);
        }

        [Fact]
        public void Given_Cancelled_Event_When_Publish_Then_Cancelled()
        {
            // arrange
            var entity = CreateMeetupEvent();
            entity.Publish();
            entity.Cancel();

            // act
            entity.Publish();

            // assert
            Assert.Equal(MeetupEventStatus.Cancelled, entity.Status);
        }

        [Fact]
        public void Given_Created_Event_When_Increase_Capacity_Then_Capacity_Increased()
        {
            // arrange
            const int increase = 10;
            var entity = CreateMeetupEvent();
            var expected = entity.Capacity + increase;
            entity.Publish();

            // act
            entity.IncreaseCapacity(increase);

            // assert
            Assert.Equal(expected, entity.Capacity);
        }

        [Fact]
        public void Given_Created_Event_When_Reduce_Capacity_Then_Capacity_Reduced()
        {
            // arrange
            const int decrease = 10;
            var entity = CreateMeetupEvent();
            var expected = entity.Capacity - decrease;
            entity.Publish();

            // act
            entity.ReduceCapacity(decrease);

            // assert
            Assert.Equal(expected, entity.Capacity);
        }

        [Fact]
        public void Given_Published_Event_When_Accept_Invitation_Then_New_Invitation_Response()
        {
            // arrange
            var entity = CreateMeetupEvent();
            entity.Publish();

            // act
            var userId = NewGuid();
            entity.AcceptInvitation(userId, DateTimeOffset.Now);

            // assert
            var invitation = entity.Invitations.FirstOrDefault(x => x.UserId == userId);
            Assert.NotNull(invitation);
            Assert.True(invitation.Going);
        }

        [Fact]
        public void Given_Published_Event_When_Decline_Invitation_Then_New_Invitation_Response()
        {
            // arrange
            var entity = CreateMeetupEvent();
            entity.Publish();

            // act
            var userId = NewGuid();
            entity.DeclineInvitation(userId);

            // assert
            var invitation = entity.Invitations.FirstOrDefault(x => x.UserId == userId);
            Assert.NotNull(invitation);
            Assert.False(invitation.Going);
        }

        [Fact]
        public void Given_Published_Event_When_Accept_Invitation_Without_Capacity_Then_Throws()
        {
            // arrange
            var entity = CreateMeetupEvent();
            entity.Publish();
            entity.ReduceCapacity(40);

            // act
            entity.AcceptInvitation(NewGuid(), DateTimeOffset.Now);
            entity.AcceptInvitation(NewGuid(), DateTimeOffset.Now);

            void AcceptInvitation() => entity.AcceptInvitation(NewGuid(), DateTimeOffset.Now);

            // assert
            Assert.Throws<ApplicationException>(AcceptInvitation);
        }


        [Fact]
        public void Given_Draft_Event_When_Accept_Invitation_Then_Throws()
        {
            // arrange
            var entity = CreateMeetupEvent();

            // act
            void AcceptInvitation() => entity.AcceptInvitation(NewGuid(), DateTimeOffset.Now);

            // assert
            Assert.Throws<ApplicationException>(AcceptInvitation);
        }

        static Domain.MeetupEvent CreateMeetupEvent(int capacity = 42)
            => new(NewGuid(), "netcorebcn", "microservices failures", capacity);
    }
}