using System;
using FluentAssertions;
using Xunit;
using Meetup.Scheduling.Domain;
using static System.Guid;

namespace Meetup.Scheduling.Test
{
    public class AttendantListTest
    {
        [Fact]
        public void Given_AttendantList_With_Enough_Capacity_When_Accept_Invitation_Then_User_Going()
        {
            // arrange
            var sut = CreateAttendantList();

            // act
            sut.Accept(joe, DateTimeOffset.Now);

            // assert
            sut.Status(joe).Should().Be(AttendantStatus.Going);
        }

        [Fact]
        public void Given_AttendantList_Event_When_Decline_Invitation_Then_User_Not_Going()
        {
            // arrange
            var sut = CreateAttendantList();

            // act
            sut.Decline(joe, DateTimeOffset.Now);

            // assert
            sut.Status(joe).Should().Be(AttendantStatus.NotGoing);
        }

        [Fact]
        public void Given_AttendantList_Meetup_Without_Enough_Capacity_When_Accept_Invitation_Then_User_Waiting()
        {
            // arrange
            var sut = CreateAttendantList(capacity: 2);
            sut.Accept(alice, DateTimeOffset.Now);
            sut.Accept(carla, DateTimeOffset.Now);

            // act
            sut.Accept(joe, DateTimeOffset.Now);

            // assert
            sut.Status(joe).Should().Be(AttendantStatus.Waiting);
        }

        [Fact]
        public void Given_AttendantList_When_Reduce_Capacity_Then_AttendantsList_Updated()
        {
            // arrange
            var sut = CreateAttendantList(capacity: 3);
            sut.Accept(alice, DateTimeOffset.Now);
            sut.Accept(carla, DateTimeOffset.Now);
            sut.Accept(joe, DateTimeOffset.Now);

            // act
            sut.ReduceCapacity(1);

            // assert
            sut.Status(alice).Should().Be(AttendantStatus.Going);
            sut.Status(carla).Should().Be(AttendantStatus.Going);
            sut.Status(joe).Should().Be(AttendantStatus.Waiting);
        }

        [Fact]
        public void Given_AttendantList_When_Increase_Capacity_Then_AttendantsList_Updated()
        {
            // arrange
            var sut = CreateAttendantList(capacity: 2);
            sut.Accept(alice, DateTimeOffset.Now);
            sut.Accept(carla, DateTimeOffset.Now);
            sut.Accept(joe, DateTimeOffset.Now);

            // act
            sut.IncreaseCapacity(1);

            // assert
            sut.Status(alice).Should().Be(AttendantStatus.Going);
            sut.Status(carla).Should().Be(AttendantStatus.Going);
            sut.Status(joe).Should().Be(AttendantStatus.Going);
        }

        Guid joe   = NewGuid();
        Guid carla = NewGuid();
        Guid alice = NewGuid();

        static AttendantList CreateAttendantList(int capacity = 42)
            => AttendantList.From(capacity);
    }
}