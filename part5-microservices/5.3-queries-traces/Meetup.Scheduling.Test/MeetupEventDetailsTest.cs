using System;
using Meetup.Scheduling.MeetupDetails;
using Xunit;
using static System.Guid;

namespace Meetup.Scheduling.Test
{
    public class MeetupEventDetailsTest
    {
        [Fact]
        public void Given_Created_Meetup_Without_Location_When_Publish_Then_Throws()
        {
            // arrange
            var sut = CreateMeetupEvent();
            sut.Schedule(ScheduleDateTime.From(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(15), 2));

            // act
            void Publish() => sut.Publish();

            // assert
            Assert.ThrowsAny<ApplicationException>(Publish);
        }

        [Fact]
        public void Given_Created_Meetup_Without_ScheduledTime_When_Publish_Then_Throws()
        {
            // arrange
            var sut = CreateMeetupEvent();
            sut.MakeOnSiteEvent("Addresss 1");

            // act
            void Publish() => sut.Publish();

            // assert
            Assert.ThrowsAny<ApplicationException>(Publish);
        }

        [Fact]
        public void Given_Published_Meetup_When_Publish_Then_Throws()
        {
            // arrange
            var sut = CreatePublishedMeetupEvent();

            // act
            void Publish() => sut.Publish();

            // assert
            Assert.ThrowsAny<ApplicationException>(Publish);
        }

        [Fact]
        public void Given_Published_Meetup_When_Cancel_Then_Event_Cancelled()
        {
            // arrange
            var sut = CreatePublishedMeetupEvent();

            // act
            sut.Cancel("test");

            // assert
            Assert.Equal(MeetupEventStatus.Cancelled, sut.Status);
        }

        [Fact]
        public void Given_Cancelled_Event_When_Publish_Then_Throws()
        {
            // arrange
            var sut = CreatePublishedMeetupEvent();
            sut.Cancel("test");

            // act
            void Publish() => sut.Publish();

            // assert
            Assert.ThrowsAny<ApplicationException>(Publish);
        }

        static MeetupDetailsAggregate CreateMeetupEvent()
        {
            var meetupAggregate = new MeetupDetailsAggregate {Id = NewGuid()};
            meetupAggregate.Create(GroupSlug.From("netcorebcn"),
                Details.From("microservices failures", "This is a talk about main microservices pitfalls.."), 2);

            return meetupAggregate;
        }

        static MeetupDetailsAggregate CreatePublishedMeetupEvent()
        {
            var sut = CreateMeetupEvent();
            sut.Schedule(ScheduleDateTime.From(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(15), 2));
            sut.MakeOnlineEvent(new Uri("https://zoom.us/netcorebcn"));
            sut.Publish();
            return sut;
        }
    }
}