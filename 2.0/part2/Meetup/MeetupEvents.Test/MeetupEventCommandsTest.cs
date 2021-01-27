using System;
using FluentAssertions;
using MeetupEvents.Application;
using MeetupEvents.Infrastructure;
using Xunit;

namespace MeetupEvents.Test
{
    public class MeetupEventCommandsTest
    {
        static MeetupEventsApplicationService Sut = new(null);

        [Fact]
        public void Given_Created_Meetup_When_Publish_Then_Published()
        {
            // arrange
            var meetup = CreateMeetup();

            // act
            Sut.Publish(meetup);

            // assert
            meetup.Status.Should().Be(MeetupEventStatus.Published);
        }


        [Fact]
        public void Given_Published_Meetup_When_Cancel_Then_Cancelled()
        {
            // arrange
            var meetup = CreateMeetup();
            Sut.Publish(meetup);

            // act
            Sut.Cancel(meetup, "covid");

            // assert
            meetup.Status.Should().Be(MeetupEventStatus.Cancelled);
        }

        [Fact]
        public void Given_Cancelled_Meetup_When_Publish_Then_InvalidOperation()
        {
            // arrange
            var meetup = CreateMeetup();
            Sut.Publish(meetup);
            Sut.Cancel(meetup, "more covid");

            // act
            Action publish = () => Sut.Publish(meetup);

            // assert
            publish.Should().ThrowExactly<InvalidOperationException>();
        }

        MeetupEvent CreateMeetup()
            => Sut.Create(Guid.NewGuid(), "Microservices failures", 10);
    }
}