using System;
using System.Linq;
using FluentAssertions;
using MeetupEvents.Domain;
using Xunit;
using static System.Guid;

namespace MeetupEvents.Test
{
    public class MeetupEventAggregateTest
    {
        [Fact]
        public void Given_Created_Meetup_When_Publish_Then_Published()
        {
            // arrange
            var meetup = CreateMeetup();

            // act
            meetup.Publish();

            // assert
            meetup.Status.Should().Be(MeetupEventStatus.Published);
        }

        [Fact]
        public void Given_Published_Meetup_When_Cancel_Then_Cancelled()
        {
            // arrange
            var meetup = CreateMeetup();
            meetup.Publish();

            // act
            meetup.Cancel("covid");

            // assert
            meetup.Status.Should().Be(MeetupEventStatus.Cancelled);
        }

        [Fact]
        public void Given_Cancelled_Meetup_When_Publish_Then_InvalidOperation()
        {
            // arrange
            var meetup = CreateMeetup();
            meetup.Publish();
            meetup.Cancel("more covid");

            // act
            Action publish = () => meetup.Publish();

            // assert
            publish.Should().ThrowExactly<InvalidOperationException>();
        }

        [Fact]
        public void Given_Published_Meetup_When_Member_Attend_Then_Accepted()
        {
            // arrange
            var meetup = CreateMeetup();
            meetup.Publish();
            var joe = NewGuid();

            // act
            meetup.Attend(joe, DateTimeOffset.UtcNow);

            // assert
            meetup.Attendants.FirstOrDefault(x => x.UserId == joe).Should().NotBeNull();
        }

        [Fact]
        public void Given_Member_Attending_When_Cancel_Attendance_Then_Removed_From_Attendants_List()
        {
            // arrange
            var meetup = CreateMeetup();
            meetup.Publish();
            var joe = NewGuid();
            meetup.Attend(joe, DateTimeOffset.UtcNow);

            // act
            meetup.CancelAttendance(joe, DateTimeOffset.UtcNow);

            // assert
            meetup.Attendants.FirstOrDefault(x => x.UserId == joe).Should().BeNull();
        }

        [Fact]
        public void Given_Meetup_With_Enough_Capacity_When_Members_Attend_Then_Going()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var meetup = CreateMeetup(capacity: 3);
            meetup.Publish();

            var joe   = NewGuid();
            var alice = NewGuid();
            var bob   = NewGuid();

            // act
            meetup.Attend(joe, now);
            meetup.Attend(alice, now.AddSeconds(1));
            meetup.Attend(bob, now.AddSeconds(2));

            // assert
            meetup.Going(joe).Should().BeTrue();
            meetup.Going(alice).Should().BeTrue();
            meetup.Going(bob).Should().BeTrue();
        }

        [Fact]
        public void Given_Meetup_Without_Enough_Capacity_When_Members_Attend_Then_Some_Waiting()
        {
            // arrange
            var now    = DateTimeOffset.UtcNow;
            var meetup = CreateMeetup(capacity: 2);
            meetup.Publish();

            var joe   = NewGuid();
            var alice = NewGuid();
            var bob   = NewGuid();

            // act
            meetup.Attend(joe, now);
            meetup.Attend(alice, now.AddSeconds(1));
            meetup.Attend(bob, now.AddSeconds(2));

            // assert
            meetup.Going(joe).Should().BeTrue();
            meetup.Going(alice).Should().BeTrue();
            meetup.Waiting(bob).Should().BeTrue();

            // act
            meetup.CancelAttendance(alice, now.AddSeconds(3));

            // assert
            meetup.Going(joe).Should().BeTrue();
            meetup.NotGoing(alice).Should().BeTrue();
            meetup.Going(bob).Should().BeTrue();
        }

        MeetupEventAggregate CreateMeetup(int capacity = 10)
        {
            var meetup = new MeetupEventAggregate();
            meetup.Create(NewGuid(), NewGuid(), "Microservices failures",
                "This is talk about all failures Ive seen with microservices ...", capacity);
            return meetup;
        }
    }

    public static class TestExtensions
    {
        public static bool Going(this MeetupEventAggregate meetup, Guid userId)
            => meetup.Attendants.Any(x => x.UserId == userId && !x.Waiting);

        public static bool NotGoing(this MeetupEventAggregate meetup, Guid userId)
            => meetup.Attendants.Any(x => x.UserId != userId);

        public static bool Waiting(this MeetupEventAggregate meetup, Guid userId)
            => meetup.Attendants.Any(x => x.UserId == userId && x.Waiting);
    }
}