using Xunit;
using Meetup.Scheduling.Domain;
using static System.Guid;

namespace Meetup.Scheduling.Test
{
    public class MeetupEventDetailsTest
    {
        [Fact]
        public void Given_Draft_Event_When_Publish_Then_Event_Scheduled()
        {
            // arrange
            var entity = CreateMeetupEvent();

            // act
            entity.Publish();

            // assert
            Assert.Equal(MeetupEventStatus.Published, entity.Status);
        }

        [Fact]
        public void Given_Scheduled_Event_When_Cancel_Then_Event_Cancelled()
        {
            // arrange
            var entity = CreateMeetupEvent();
            entity.Publish();

            // act
            entity.Cancel();

            // assert
            Assert.Equal(MeetupEventStatus.Cancelled, entity.Status);
        }

        [Fact]
        public void Given_Cancelled_Event_When_Publish_Then_Cancelled()
        {
            // arrange
            var entity = CreateMeetupEvent();
            entity.Publish();
            entity.Cancel();

            // act
            entity.Publish();

            // assert
            Assert.Equal(MeetupEventStatus.Cancelled, entity.Status);
        }

        static MeetupEventDetailsAggregate CreateMeetupEvent()
            => new(NewGuid(), GroupSlug.From("netcorebcn"), Details.From("microservices failures", "This is a talk about main microservices pitfalls.."));
    }
}