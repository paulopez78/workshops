using System;
using System.Linq;
using FluentAssertions;
using Meetup.Scheduling.Domain;
using Xunit;
using static System.Guid;
using static Meetup.Scheduling.Domain.Events.V1.AttendantList;

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
            var expected = sut.AcceptInvitation(joe);

            // assert
            sut.Going(joe).Should().Be(expected);
        }

        [Fact]
        public void Given_Opened_AttendantList_With_Enough_Capacity_When_Decline_Invitation_Then_Attendant_NotGoing()
        {
            // arrange
            var sut = CreateOpenedAttendantList();

            // act
            var expected = sut.DeclineInvitation(joe);

            // assert
            sut.NotGoing(joe).Should().Be(expected);
        }

        [Fact]
        public void Given_Opened_Attendant_List_Without_Enough_Capacity_When_Accept_Invitation_Then_User_Waiting()
        {
            // arrange
            var sut = CreateOpenedAttendantList(capacity: 2);

            var aliceExpected = sut.AcceptInvitation(alice);
            var carlaExpected = sut.AcceptInvitation(carla);

            // act
            var joeExpected = sut.AcceptInvitation(joe);

            // assert
            sut.Going(alice).Should().Be(aliceExpected);
            sut.Going(carla).Should().Be(carlaExpected);
            sut.Waiting(joe).Should().Be(joeExpected);
        }

        [Fact]
        public void Given_Opened_AttendantList_When_Reduce_Capacity_Then_Some_Attendants_Waiting()
        {
            // arrange
            var sut           = CreateOpenedAttendantList(capacity: 3);
            var aliceExpected = sut.AcceptInvitation(alice);
            var carlaExpected = sut.AcceptInvitation(carla);
            var joeExpected   = sut.AcceptInvitation(joe);

            // act
            sut.ReduceCapacity(1);

            // assert
            sut.Going(alice).Should().Be(aliceExpected);
            sut.Going(carla).Should().Be(carlaExpected);
            sut.Going(joe).Should().Be(joeExpected);

            sut.Waiting(joe).Should().Be(joeExpected);
        }

        [Fact]
        public void Given_Opened_AttendantList_When_Increase_Capacity_Then_All_Attendants_Going()
        {
            // arrange
            var sut           = CreateOpenedAttendantList(capacity: 2);
            var aliceExpected = sut.AcceptInvitation(alice);
            var carlaExpected = sut.AcceptInvitation(carla);
            var joeExpected   = sut.AcceptInvitation(joe);

            // act
            sut.IncreaseCapacity(1);

            // assert
            sut.Going(alice).Should().Be(aliceExpected);
            sut.Going(carla).Should().Be(carlaExpected);
            sut.Waiting(joe).Should().Be(joeExpected);
            sut.Going(joe).Should().Be(joeExpected);
        }

        Guid joe   = NewGuid();
        Guid carla = NewGuid();
        Guid alice = NewGuid();

        static AttendantListAggregate CreateAttendantList(int capacity = 42)
            => new(NewGuid(), capacity);

        static AttendantListAggregate CreateOpenedAttendantList(int capacity = 42)
        {
            var sut = CreateAttendantList(capacity);
            sut.Open();
            return sut;
        }
    }

    public static class AttendantListAggregateTestExtensions
    {
        public static Events.V1.AttendantList.Attendant AcceptInvitation(this AttendantListAggregate sut, Guid user)
        {
            var userAcceptedAt = DateTimeOffset.Now;
            var expected       = new Events.V1.AttendantList.Attendant(sut.Id, user, userAcceptedAt);
            sut.AcceptInvitation(user, userAcceptedAt);
            return expected;
        }

        public static Events.V1.AttendantList.Attendant DeclineInvitation(this AttendantListAggregate sut, Guid user)
        {
            var userAcceptedAt = DateTimeOffset.Now;
            var expected       = new Events.V1.AttendantList.Attendant(sut.Id, user, userAcceptedAt);
            sut.DeclineInvitation(user, userAcceptedAt);
            return expected;
        }

        public static Events.V1.AttendantList.Attendant Going(this AttendantListAggregate sut, Guid userId)
            => sut.Changes.OfType<AttendantMovedToGoingList>().FirstOrDefault(x => x.Attendant.UserId == userId)
                ?.Attendant;

        public static Events.V1.AttendantList.Attendant Waiting(this AttendantListAggregate sut, Guid userId)
            => sut.Changes.OfType<AttendantMovedToWaitingList>().FirstOrDefault(x => x.Attendant.UserId == userId)
                ?.Attendant;

        public static Events.V1.AttendantList.Attendant NotGoing(this AttendantListAggregate sut, Guid userId)
            => sut.Changes.OfType<AttendantMovedToNotGoingList>().FirstOrDefault(x => x.Attendant.UserId == userId)
                ?.Attendant;
    }
}