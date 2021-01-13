using System;
using Xunit;
using Meetup.Scheduling.Domain;
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
            var sut= CreatePublishedMeetupEvent();

            // act
            sut.Cancel();

            // assert
            Assert.Equal(MeetupEventStatus.Cancelled, sut.Status);
        }

        [Fact]
        public void Given_Cancelled_Event_When_Publish_Then_Throws()
        {
            // arrange
            var sut = CreatePublishedMeetupEvent();
            sut.Cancel();

            // act
            void Publish() => sut.Publish();

            // assert
            Assert.ThrowsAny<ApplicationException>(Publish);
        }

        static MeetupEventDetailsAggregate CreateMeetupEvent()
            => new(NewGuid(), GroupSlug.From("netcorebcn"), Details.From("microservices failures", "This is a talk about main microservices pitfalls.."));

        static MeetupEventDetailsAggregate CreatePublishedMeetupEvent()
        {
            var sut = CreateMeetupEvent();
            sut.Schedule(ScheduleDateTime.From(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(15), 2));
            sut.MakeOnlineEvent(new Uri("https://zoom.us/netcorebcn"));
            sut.Publish();
            return sut;
        }
    }
}