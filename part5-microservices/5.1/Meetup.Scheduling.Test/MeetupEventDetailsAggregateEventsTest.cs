using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using Meetup.Scheduling.MeetupDetails;
using static System.Guid;
using static Meetup.Scheduling.MeetupDetails.Events.V1;

namespace Meetup.Scheduling.Test
{
    public class MeetupEventDetailsProjectionTest
    {
        [Fact]
        public void Given_Meetup_Published_Events_When_Applying_Then_Meetup_Aggregate_Projected()
        {
            // arrange
            var now              = DateTimeOffset.Now;
            var sut              = new MeetupEventDetailsAggregate {Id = NewGuid()};
            var expectedDetails  = Details.From("microservices failures", "This is a talk about ..");
            var expectedGroup    = GroupSlug.From("netcorebcn");
            var expectedSchedule = ScheduleDateTime.From(now, now, now.AddHours(2));
            var expectedLocation = Location.Online(new Uri("https://zoom.us/netcorebcn"));

            var events = new List<object>
            {
                new Created(sut.Id, expectedGroup, expectedDetails.Title, expectedDetails.Description, 2),
                new Scheduled(sut.Id, now, now.AddHours(2)),
                new MadeOnline(sut.Id, expectedLocation.Url.ToString()),
                new Published(sut.Id, expectedGroup)
            };

            // act
            foreach (var domainEvent in events)
            {
                sut.Apply(domainEvent);
            }

            // assert

            sut.Details.Should().Be(expectedDetails);
            sut.Group.Should().Be(expectedGroup);
            sut.Location.Should().Be(expectedLocation);
            sut.ScheduleTime.Should().Be(expectedSchedule);
            sut.Status.Should().Be(MeetupEventStatus.Published);
        }
    }
}