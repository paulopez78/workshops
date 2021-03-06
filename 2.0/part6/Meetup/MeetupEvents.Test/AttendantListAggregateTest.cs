﻿using System;
using System.Linq;
using FluentAssertions;
using MeetupEvents.Domain;
using Xunit;
using static System.Guid;

namespace MeetupEvents.Test
{
    public class AttendantListAggregateTest
    {
        [Fact]
        public void Given_Created_AttendantList_When_Open_Then_Opened()
        {
            // arrange
            var attendantList = CreateAttendantList();

            // act
            attendantList.Open(DateTimeOffset.UtcNow);

            // assert
            attendantList.Status.Should().Be(AttendantListStatus.Opened);
        }

        [Fact]
        public void Given_Opened_AttendantList_When_Close_Then_Closed()
        {
            // arrange
            var attendantList = CreateAttendantList();
            attendantList.Open(DateTimeOffset.UtcNow);

            // act
            attendantList.Close(DateTimeOffset.UtcNow);

            // assert
            attendantList.Status.Should().Be(AttendantListStatus.Closed);
        }

        [Fact]
        public void Given_Closed_AttendantList_When_Open_Then_Opened()
        {
            // arrange
            var attendantList = CreateAttendantList();
            attendantList.Open(DateTimeOffset.UtcNow);
            attendantList.Close(DateTimeOffset.UtcNow);

            // act
            attendantList.Open(DateTimeOffset.UtcNow);

            // assert
            attendantList.Status.Should().Be(AttendantListStatus.Opened);
        }

        [Fact]
        public void Given_Created_AttendantList_When_Close_Then_InvalidOperation()
        {
            // arrange
            var attendantList = CreateAttendantList();

            // act
            Action close = () => attendantList.Close(DateTimeOffset.UtcNow);

            // assert
            close.Should().ThrowExactly<InvalidOperationException>();
        }


        [Fact]
        public void Given_Opened_AttendantList_When_Member_Attend_Then_Going()
        {
            // arrange
            var attendantList = CreateAttendantList();
            attendantList.Open(DateTimeOffset.UtcNow);
            var joe = NewGuid();

            // act
            attendantList.Attend(joe, DateTimeOffset.UtcNow);

            // assert
            attendantList.Going(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Member_Attending_When_Cancel_Attendance_Then_Removed_From_Attendants_List()
        {
            // arrange
            var attendantList = CreateAttendantList();
            attendantList.Open(DateTimeOffset.UtcNow);

            var joe = NewGuid();
            attendantList.Attend(joe, DateTimeOffset.UtcNow);

            // act
            attendantList.CancelAttendance(joe, DateTimeOffset.UtcNow);

            // assert
            attendantList.NotGoing(joe).Should().BeTrue();
        }

        [Fact]
        public void Given_Attendant_List_With_Enough_Capacity_When_Members_Attend_Then_Going()
        {
            // arrange
            var attendantList = CreateAttendantList();
            attendantList.Open(DateTimeOffset.UtcNow);

            var now = DateTimeOffset.UtcNow;

            // act
            attendantList.Attend(joe, now);
            attendantList.Attend(alice, now.AddSeconds(1));
            attendantList.Attend(bob, now.AddSeconds(2));

            // assert
            attendantList.Going(joe).Should().BeTrue();
            attendantList.Going(alice).Should().BeTrue();
            attendantList.Going(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_AttendingList_Without_Enough_Capacity_When_Members_Attend_Then_Some_Waiting()
        {
            // arrange
            var now           = DateTimeOffset.UtcNow;
            var attendantList = CreateAttendantList(capacity: 2);
            attendantList.Open(DateTimeOffset.UtcNow);

            // act
            attendantList.Attend(joe, now);
            attendantList.Attend(alice, now.AddSeconds(1));
            attendantList.Attend(bob, now.AddSeconds(2));

            // assert
            attendantList.Going(joe).Should().BeTrue();
            attendantList.Going(alice).Should().BeTrue();
            attendantList.Waiting(bob).Should().BeTrue();

            // act
            attendantList.CancelAttendance(alice, now.AddSeconds(3));

            // assert
            attendantList.Going(joe).Should().BeTrue();
            attendantList.NotGoing(alice).Should().BeTrue();
            attendantList.Going(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Full_Attendant_List_When_ReduceCapacity_Then_Last_Attendant_Waiting()
        {
            // arrange
            var attendantList = CreateAttendantList(capacity: 3);
            attendantList.Open(DateTimeOffset.UtcNow);

            var now = DateTimeOffset.UtcNow;
            attendantList.Attend(joe, now);
            attendantList.Attend(alice, now.AddSeconds(1));
            attendantList.Attend(bob, now.AddSeconds(2));

            // act
            attendantList.ReduceCapacity(1);

            // assert
            attendantList.Going(joe).Should().BeTrue();
            attendantList.Going(alice).Should().BeTrue();
            attendantList.Waiting(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Attendant_List_With_Waiting_Attendant_When_IncreaseCapacity_Then_Attendant_Going()
        {
            // arrange
            var attendantList = CreateAttendantList(capacity: 2);
            attendantList.Open(DateTimeOffset.UtcNow);

            var now = DateTimeOffset.UtcNow;
            attendantList.Attend(joe, now);
            attendantList.Attend(alice, now.AddSeconds(1));
            attendantList.Attend(bob, now.AddSeconds(2));

            // act
            attendantList.IncreaseCapacity(1);

            // assert
            attendantList.Going(joe).Should().BeTrue();
            attendantList.Going(alice).Should().BeTrue();
            attendantList.Going(bob).Should().BeTrue();
        }

        private AttendantListAggregate CreateAttendantList(int capacity = 0)
        {
            var aggregate = new AttendantListAggregate();
            aggregate.Create(NewGuid(), NewGuid(), capacity, 10);
            return aggregate;
        }

        Guid joe   = NewGuid();
        Guid alice = NewGuid();
        Guid bob   = NewGuid();
    }

    public static class TestExtensions
    {
        public static bool Going(this AttendantListAggregate attendantList, Guid userId)
            => attendantList.Attendants.Any(x => x.UserId == userId && !x.Waiting);

        public static bool NotGoing(this AttendantListAggregate attendantList, Guid userId)
            => attendantList.Attendants.All(x => x.UserId != userId);

        public static bool Waiting(this AttendantListAggregate attendantList, Guid userId)
            => attendantList.Attendants.Any(x => x.UserId == userId && x.Waiting);
    }
}