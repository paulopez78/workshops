using System;
using Xunit;
using Meetup.Scheduling.Domain;
using static System.Guid;

namespace Meetup.Scheduling.Test
{
    public class AttendantListTest 
    {
        [Fact]
        public void Given_Event_With_Capacity_When_Accept_Invitation_Then_User_Going()
        {
            // arrange
            var sut = CreateAttendantList();
        
            // act
            sut.Accept(joe, DateTimeOffset.Now);
        
            // assert
            Assert.Equal(AttendantStatus.Going, sut.Status(joe));
        }
        
        [Fact]
        public void Given_Published_Event_When_Decline_Invitation_Then_User_Not_Going()
        {
            // arrange
            var sut = CreateAttendantList();
        
            // act
            sut.Decline(joe, DateTimeOffset.Now);
        
            // assert
            Assert.Equal(AttendantStatus.NotGoing, sut.Status(joe));
        }
        
        [Fact]
        public void Given_Meetup_Without_Capacity_When_Accept_Invitation_Then_User_Waiting()
        {
            // arrange
            var sut = CreateAttendantList(capacity: 2);
            sut.Accept(alice, DateTimeOffset.Now);
            sut.Accept(carla, DateTimeOffset.Now);
        
            // act
            sut.Accept(joe, DateTimeOffset.Now);
        
            // assert
            Assert.Equal(AttendantStatus.Waiting, sut.Status(joe));
        }
        
        [Fact]
        public void Given_Meetup_Event_When_Reduce_Capacity_Then_AttendantsList_Updated()
        {
            // arrange
            var sut = CreateAttendantList(capacity: 3);
            sut.Accept(alice, DateTimeOffset.Now);
            sut.Accept(carla, DateTimeOffset.Now);
            sut.Accept(joe, DateTimeOffset.Now);
        
            // act
            sut.ReduceCapacity(1);
        
            // assert
            Assert.Equal(AttendantStatus.Going, sut.Status(alice));
            Assert.Equal(AttendantStatus.Going, sut.Status(carla));
            Assert.Equal(AttendantStatus.Waiting, sut.Status(joe));
        }
        
        [Fact]
        public void Given_Meetup_Event_When_Increase_Capacity_Then_AttendantsList_Updated()
        {
            // arrange
            var sut = CreateAttendantList(capacity: 2);
            sut.Accept(alice, DateTimeOffset.Now);
            sut.Accept(carla, DateTimeOffset.Now);
            sut.Accept(joe, DateTimeOffset.Now);
        
            // act
            sut.IncreaseCapacity(1);
        
            // assert
            Assert.Equal(AttendantStatus.Going, sut.Status(alice));
            Assert.Equal(AttendantStatus.Going, sut.Status(carla));
            Assert.Equal(AttendantStatus.Going, sut.Status(joe));
        }
        
        Guid joe = NewGuid();
        Guid carla = NewGuid();
        Guid alice = NewGuid();
        
        static AttendantList CreateAttendantList(int capacity = 42)
            => AttendantList.From(capacity);
    }
}