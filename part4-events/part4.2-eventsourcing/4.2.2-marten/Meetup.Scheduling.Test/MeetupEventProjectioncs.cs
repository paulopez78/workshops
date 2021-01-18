using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using Meetup.Scheduling.Queries;
using static System.Guid;
using MeetupEvents = Meetup.Scheduling.MeetupDetails.Events.V1;
using AttendantListEvents = Meetup.Scheduling.AttendantList.Events.V1;

namespace Meetup.Scheduling.Test
{
    public class MeetupEventProjectionTest
    {
        [Fact]
        public void Given_Meetup_Events_When_Applying_Then_State_Projected()
        {
            // arrange
            var now = DateTimeOffset.Now;
            var id  = NewGuid();

            const string title       = "microservices failures";
            const string description = "This is a talk about ..";
            const string group       = "netcorebcn";
            const string location    = "https://zoom.us/netcorebcn";

            Guid joe   = NewGuid();
            Guid alice = NewGuid();
            Guid carla = NewGuid();
            
            var events = new List<object>
            {
                new MeetupEvents.Created(id, group, title, description, 2),
                new MeetupEvents.Scheduled(id, now, now.AddHours(2)),
                new MeetupEvents.MadeOnline(id, location),
                new MeetupEvents.Published(id),
                new AttendantListEvents.Created(id, 2),
                new AttendantListEvents.Opened(id),
                new AttendantListEvents.AttendantAdded(id, joe, now.AddSeconds(1)),
                new AttendantListEvents.AttendantAdded(id, alice, now.AddSeconds(2)),
                new AttendantListEvents.AttendantWaitingAdded(id, carla, now.AddSeconds(3)),
                new AttendantListEvents.CapacityIncreased(id, 1),
                new AttendantListEvents.AttendantsRemovedFromWaitingList(id, now.AddSeconds(4), carla),
                new AttendantListEvents.CapacityReduced(id, 1),
                new AttendantListEvents.AttendantsAddedToWaitingList(id, now.AddSeconds(5), carla),
                new AttendantListEvents.AttendantRemoved(id, alice, now.AddSeconds(2)),
                new AttendantListEvents.AttendantsRemovedFromWaitingList(id, now.AddSeconds(6), carla),
            };

            // act
            var result = events.Aggregate<object, MeetupEvent>(null, MeetupEventProjection.When);

            // assert
            result.Title.Should().Be(title);
            result.Group.Should().Be(group);
            result.Status.Should().Be("Published");
            result.Attendants.Should().HaveCount(2);

            result.Attendants.FirstOrDefault(x => x.UserId == alice).Should().BeNull();
            result.Attendants.FirstOrDefault(x => x.UserId == joe)?.Waiting.Should().BeFalse();
            result.Attendants.FirstOrDefault(x => x.UserId == carla)?.Waiting.Should().BeFalse();
        }
    }
}