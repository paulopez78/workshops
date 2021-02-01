using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Meetup.Scheduling.MeetupDetails;
using Xunit;
using static System.Guid;
using MeetupEvents = Meetup.Scheduling.MeetupDetails.Events.V1;
using AttendantListEvents = Meetup.Scheduling.AttendantList.Events.V1;

namespace Meetup.Scheduling.Test
{
    public class MeetupEventProjectionTest
    {
        [Fact]
        public void Should_Project()
        {
            // arrange
            var now = DateTimeOffset.Now;
            var id  = NewGuid();

            const string title       = "microservices failures";
            const string description = "This is a talk about ..";
            const string group       = "netcorebcn";
            const string url         = "https://zoom.us/netcorebcn";

            var events = new List<object>
            {
                new MeetupEvents.Created(id, group, title, description, 2),
                new MeetupEvents.Scheduled(id, now, now.AddHours(2)),
                new MeetupEvents.MadeOnline(id, url),
                new MeetupEvents.Published(id, group),
            };

            // act
            var result =
                events.Aggregate<object, MeetupDetailsEventProjection.MeetupDetailsEvent>(null,
                    MeetupDetailsEventProjection.When);

            // assert
            result.Title.Should().Be(title);
            result.Group.Should().Be(group);
            result.Description.Should().Be(description);
            result.Start.Should().Be(now);
            result.End.Should().Be(now.AddHours(2));
            result.Online.Should().BeTrue();
            result.Location.Should().Be(url);
            result.Status.Should().Be("Published");
        }
    }
}