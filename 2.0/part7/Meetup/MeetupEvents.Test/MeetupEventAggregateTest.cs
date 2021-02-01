using System;
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
            meetup.Publish(DateTimeOffset.UtcNow);

            // assert
            meetup.Status.Should().Be(MeetupEventStatus.Published);
        }

        [Fact]
        public void Given_Published_Meetup_When_Cancel_Then_Cancelled()
        {
            // arrange
            var meetup = CreateMeetup();
            meetup.Publish(DateTimeOffset.UtcNow);

            // act
            meetup.Cancel("covid", DateTimeOffset.UtcNow);

            // assert
            meetup.Status.Should().Be(MeetupEventStatus.Cancelled);
        }

        [Fact]
        public void Given_Cancelled_Meetup_When_Publish_Then_InvalidOperation()
        {
            // arrange
            var meetup = CreateMeetup();
            meetup.Publish(DateTimeOffset.UtcNow);
            meetup.Cancel("more covid", DateTimeOffset.UtcNow);

            // act
            Action publish = () => meetup.Publish(DateTimeOffset.UtcNow);

            // assert
            publish.Should().ThrowExactly<InvalidOperationException>();
        }


        MeetupEventAggregate CreateMeetup()
        {
            var now    = DateTimeOffset.UtcNow;
            var meetup = new MeetupEventAggregate();
            meetup.Create(
                id: NewGuid(),
                groupId: NewGuid(),
                Details.From(
                    "Microservices failures",
                    "This is talk about all failures Ive seen with microservices ..."
                )
            );

            meetup.MakeOnline(new Uri("http://zoom.us/netcorebcn"));
            meetup.Schedule(ScheduleTime.From(() => now, now.AddDays(7), 2));

            return meetup;
        }
    }
}