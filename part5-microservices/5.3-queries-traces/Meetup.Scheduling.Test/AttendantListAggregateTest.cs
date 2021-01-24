using System;
using System.Linq;
using FluentAssertions;
using Meetup.Scheduling.AttendantList;
using Xunit;
using static System.Guid;

namespace Meetup.Scheduling.Test
{
    public class AttendantListAggregateTest
    {
        [Fact]
        public void Given_Opened_AttendantList_With_Enough_Capacity_When_Accept_Invitation_Then_Attendant_Going()
        {
            // arrange
            var sut = CreateOpenedAttendantList();

            // act
            sut.Add(joe);

            // assert
            sut.Going(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Opened_AttendantList_With_Enough_Capacity_When_Decline_Invitation_Then_Attendant_NotGoing()
        {
            // arrange
            var sut = CreateOpenedAttendantList();

            // act
            sut.Add(joe);
            sut.Remove(joe);

            // assert
            sut.NotGoing(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Opened_Attendant_List_Without_Enough_Capacity_When_Accept_Invitation_Then_User_Waiting()
        {
            // arrange
            var sut = CreateOpenedAttendantList(capacity: 2);

            sut.Add(alice);
            sut.Add(carla);

            // act
            sut.Add(joe);

            // assert
            sut.Going(alice).Should().BeTrue();
            sut.Going(carla).Should().BeTrue();
            sut.Waiting(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Opened_AttendantList_When_Reduce_Capacity_Then_Some_Attendants_Waiting()
        {
            // arrange
            var sut = CreateOpenedAttendantList(capacity: 3);
            sut.Add(alice);
            sut.Add(carla);
            sut.Add(joe);

            // act
            sut.ReduceCapacity(1, DateTimeOffset.UtcNow);

            // assert
            sut.Going(alice).Should().BeTrue();
            sut.Going(carla).Should().BeTrue();
            sut.Waiting(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Opened_AttendantList_When_Increase_Capacity_Then_All_Attendants_Going()
        {
            // arrange
            var sut = CreateOpenedAttendantList(capacity: 2);
            sut.Add(alice);
            sut.Add(carla);
            sut.Add(joe);

            // act
            sut.IncreaseCapacity(1, DateTimeOffset.UtcNow);

            // assert
            sut.Going(alice).Should().BeTrue();
            sut.Going(carla).Should().BeTrue();
            sut.Going(joe).Should().BeTrue();
        }

        Guid joe   = NewGuid();
        Guid carla = NewGuid();
        Guid alice = NewGuid();

        static AttendantListAggregate CreateAttendantList(int capacity = 42)
        {
            var attendantList = new AttendantListAggregate {Id = NewGuid()};
            attendantList.Create(NewGuid(), capacity);
            return attendantList;
        }

        static AttendantListAggregate CreateOpenedAttendantList(int capacity = 42)
        {
            var sut = CreateAttendantList(capacity);
            sut.Open(DateTimeOffset.UtcNow);
            return sut;
        }
    }

    public static class AttendantListAggregateTestExtensions
    {
        public static void Add(this AttendantListAggregate sut, Guid user)
        {
            var userAcceptedAt = DateTimeOffset.UtcNow;
            sut.Add(user, userAcceptedAt);
        }

        public static void Remove(this AttendantListAggregate sut, Guid user)
        {
            var userAcceptedAt = DateTimeOffset.UtcNow;
            sut.Remove(user, userAcceptedAt);
        }
    }
}