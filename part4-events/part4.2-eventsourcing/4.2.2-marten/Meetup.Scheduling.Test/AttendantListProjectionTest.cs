using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Meetup.Scheduling.AttendantList;
using Meetup.Scheduling.MeetupDetails;
using Xunit;
using static System.Guid;
using MeetupEvents = Meetup.Scheduling.MeetupDetails.Events.V1;
using AttendantListEvents = Meetup.Scheduling.AttendantList.Events.V1;

namespace Meetup.Scheduling.Test
{
    public class AttendantListProjectionTest
    {
        [Fact]
        public void Should_Project()
        {
            // arrange
            var now      = DateTimeOffset.Now;
            var id       = NewGuid();
            var meetupId = NewGuid();

            var joe   = NewGuid();
            var alice = NewGuid();
            var carla = NewGuid();

            var events = new List<object>
            {
                new AttendantListEvents.AttendantListCreated(id, meetupId, 2),
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
            var result =
                events.Aggregate<object, AttendantListProjection.AttendantList>(null, AttendantListProjection.When);

            // assert
            result.Attendants.Should().HaveCount(2);
            Attendant(alice).Should().BeNull();
            Attendant(joe).Waiting.Should().BeFalse();
            Attendant(carla).Waiting.Should().BeFalse();

            AttendantListProjection.AttendantList.Attendant Attendant(Guid userId) =>
                result.Attendants.FirstOrDefault(x => x.UserId == userId);
        }
    }
}