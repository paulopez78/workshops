using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Meetup.Scheduling.Domain;
using static System.Guid;

namespace Meetup.Scheduling.Test
{
    public class AttendantListTest 
    {
        [Fact]
        public void Given_Draft_Event_When_Accept_Invitation_Then_Throws()
        {
            // arrange
            var entity = CreateAttendantList();
        
            // act
            void AcceptInvitation() => entity.AcceptInvitation(NewGuid(), DateTimeOffset.Now);
        
            // assert
            // Status is not part of the aggregate, how to enforce the constraint? 
            // Assert.Throws<ApplicationException>(AcceptInvitation);
        }
        
        [Fact]
        public void Given_Event_With_Capacity_When_Accept_Invitation_Then_User_Going()
        {
            // arrange
            var entity = CreateAttendantList();
        
            // act
            entity.AcceptInvitation(joe, DateTimeOffset.Now);
        
            // assert
            Assert.Equal(AttendantStatus.Going, entity.AttendantList.Attendants.Status(joe));
        }
        
        [Fact]
        public void Given_Published_Event_When_Decline_Invitation_Then_User_Not_Going()
        {
            // arrange
            var entity = CreateAttendantList();
        
            // act
            entity.DeclineInvitation(joe, DateTimeOffset.Now);
        
            // assert
            Assert.Equal(AttendantStatus.NotGoing, entity.AttendantList.Attendants.Status(joe));
        }
        
        [Fact]
        public void Given_Meetup_Without_Capacity_When_Accept_Invitation_Then_User_Waiting()
        {
            // arrange
            var entity = CreateAttendantList(capacity: 2);
            entity.AcceptInvitation(alice, DateTimeOffset.Now);
            entity.AcceptInvitation(carla, DateTimeOffset.Now);
        
            // act
            entity.AcceptInvitation(joe, DateTimeOffset.Now);
        
            // assert
            Assert.Equal(AttendantStatus.Waiting, entity.AttendantList.Attendants.Status(joe));
        }
        
        [Fact]
        public void Given_Meetup_Event_When_Reduce_Capacity_Then_AttendantsList_Updated()
        {
            // arrange
            var entity = CreateAttendantList(capacity: 3);
            entity.AcceptInvitation(alice, DateTimeOffset.Now);
            entity.AcceptInvitation(carla, DateTimeOffset.Now);
            entity.AcceptInvitation(joe, DateTimeOffset.Now);
        
            // act
            entity.ReduceCapacity(1);
        
            // assert
            Assert.Equal(AttendantStatus.Going, entity.AttendantList.Attendants.Status(alice));
            Assert.Equal(AttendantStatus.Going, entity.AttendantList.Attendants.Status(carla));
            Assert.Equal(AttendantStatus.Waiting, entity.AttendantList.Attendants.Status(joe));
        }
        
        [Fact]
        public void Given_Meetup_Event_When_Increase_Capacity_Then_AttendantsList_Updated()
        {
            // arrange
            var entity = CreateAttendantList(capacity: 2);
            entity.AcceptInvitation(alice, DateTimeOffset.Now);
            entity.AcceptInvitation(carla, DateTimeOffset.Now);
            entity.AcceptInvitation(joe, DateTimeOffset.Now);
        
            // act
            entity.IncreaseCapacity(1);
        
            // assert
            Assert.Equal(AttendantStatus.Going, entity.AttendantList.Attendants.Status(alice));
            Assert.Equal(AttendantStatus.Going, entity.AttendantList.Attendants.Status(carla));
            Assert.Equal(AttendantStatus.Going, entity.AttendantList.Attendants.Status(joe));
        }
        
        Guid joe = NewGuid();
        Guid carla = NewGuid();
        Guid alice = NewGuid();
        
        static AttendantListAggregate CreateAttendantList(int capacity = 42)
            => new(NewGuid(), capacity);
    }

    public static class AttendantListTestExtensions
    {
        public static AttendantStatus? Status(this IEnumerable<Attendant> attendants, Guid userId) =>
            attendants.FirstOrDefault(x => x.UserId == userId)?.Status;
    }
}