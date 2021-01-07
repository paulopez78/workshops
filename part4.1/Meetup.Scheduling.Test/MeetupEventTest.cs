using System;
using System.Collections.Generic;
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
            entity.Publish();

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
            var invitation = entity.Attendants.FirstOrDefault(x => x.UserId == userId);
            Assert.NotNull(invitation);
            Assert.Equal(AttendantStatus.Going, invitation.Status);
        }

        [Fact]
        public void Given_Published_Event_When_Decline_Invitation_Then_New_Invitation_Response()
        {
            // arrange
            var entity = CreateMeetupEvent();
            entity.Publish();

            // act
            var userId = NewGuid();
            entity.DeclineInvitation(userId, DateTimeOffset.Now);

            // assert
            var invitation = entity.Attendants.FirstOrDefault(x => x.UserId == userId);
            Assert.NotNull(invitation);
            Assert.Equal(AttendantStatus.NotGoing, invitation.Status);
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

        [Fact]
        public void Given_Event_With_Capacity_When_Accept_Invitation_Then_User_Going()
        {
            // arrange
            var entity = CreatePublishedEvent();

            // act
            entity.AcceptInvitation(joe, DateTimeOffset.Now);

            // assert
            Assert.Equal(AttendantStatus.Going, entity.Attendants.Status(joe));
        }

        [Fact]
        public void Given_Published_Event_When_Decline_Invitation_Then_User_Not_Going()
        {
            // arrange
            var entity = CreatePublishedEvent();

            // act
            entity.DeclineInvitation(joe, DateTimeOffset.Now);

            // assert
            Assert.Equal(AttendantStatus.NotGoing, entity.Attendants.Status(joe));
        }

        [Fact]
        public void Given_Meetup_Without_Capacity_When_Accept_Invitation_Then_User_Waiting()
        {
            // arrange
            var entity = CreatePublishedEvent();
            entity.AcceptInvitation(alice, DateTimeOffset.Now);
            entity.AcceptInvitation(carla, DateTimeOffset.Now);

            // act
            entity.AcceptInvitation(joe, DateTimeOffset.Now);

            // assert
            Assert.Equal(AttendantStatus.Waiting, entity.Attendants.Status(joe));
        }

        [Fact]
        public void Given_Meetup_Event_When_Reduce_Capacity_Then_AttendantsList_Updated()
        {
            // arrange
            var entity = CreatePublishedEvent(capacity: 3);
            entity.AcceptInvitation(alice, DateTimeOffset.Now);
            entity.AcceptInvitation(carla, DateTimeOffset.Now);
            entity.AcceptInvitation(joe, DateTimeOffset.Now);

            // act
            entity.ReduceCapacity(1);

            // assert
            Assert.Equal(AttendantStatus.Going, entity.Attendants.Status(alice));
            Assert.Equal(AttendantStatus.Going, entity.Attendants.Status(carla));
            Assert.Equal(AttendantStatus.Waiting, entity.Attendants.Status(joe));
        }

        [Fact]
        public void Given_Meetup_Event_When_Increase_Capacity_Then_AttendantsList_Updated()
        {
            // arrange
            var entity = CreatePublishedEvent(capacity: 2);
            entity.AcceptInvitation(alice, DateTimeOffset.Now);
            entity.AcceptInvitation(carla, DateTimeOffset.Now);
            entity.AcceptInvitation(joe, DateTimeOffset.Now);

            // act
            entity.IncreaseCapacity(1);

            // assert
            Assert.Equal(AttendantStatus.Going, entity.Attendants.Status(alice));
            Assert.Equal(AttendantStatus.Going, entity.Attendants.Status(carla));
            Assert.Equal(AttendantStatus.Going, entity.Attendants.Status(joe));
        }

        Guid joe = NewGuid();
        Guid carla = NewGuid();
        Guid alice = NewGuid();

        static MeetupEvent CreateMeetupEvent(int capacity = 42)
            => new(NewGuid(), "netcorebcn", "microservices failures", capacity);

        static MeetupEvent CreatePublishedEvent(int capacity = 2)
        {
            var meetupEvent = CreateMeetupEvent(capacity);
            meetupEvent.Publish();
            return meetupEvent;
        }
    }

    public static class MeetupEventTestExtensions
    {
        public static AttendantStatus? Status(this IEnumerable<Attendant> attendants, Guid userId) =>
            attendants.FirstOrDefault(x => x.UserId == userId)?.Status;
    }
}