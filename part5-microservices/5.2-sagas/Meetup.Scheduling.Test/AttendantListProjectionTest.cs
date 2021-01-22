using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using static System.Guid;
using MeetupEvents = Meetup.Scheduling.Contracts.MeetupDetailsEvents.V1;
using AttendantListEvents = Meetup.Scheduling.Contracts.AttendantListEvents.V1;
using static Meetup.Scheduling.Contracts.ReadModels.V1;
using static Meetup.Scheduling.AttendantList.AttendantListProjection;

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
                new AttendantListEvents.Opened(id, meetupId, now),
                new AttendantListEvents.AttendantAdded(id, meetupId, joe, now.AddSeconds(1)),
                new AttendantListEvents.AttendantAdded(id, meetupId, alice, now.AddSeconds(2)),
                new AttendantListEvents.AttendantAddedToWaitingList(id, meetupId, carla, now.AddSeconds(3)),
                new AttendantListEvents.CapacityIncreased(id, meetupId, 1),
                new AttendantListEvents.AttendantsRemovedFromWaitingList(id, meetupId, now.AddSeconds(4), carla),
                new AttendantListEvents.CapacityReduced(id, meetupId, 1),
                new AttendantListEvents.AttendantsAddedToWaitingList(id, meetupId, now.AddSeconds(5), carla),
                new AttendantListEvents.AttendantRemoved(id, meetupId, alice, now.AddSeconds(2)),
                new AttendantListEvents.AttendantsRemovedFromWaitingList(id, meetupId, now.AddSeconds(6), carla),
            };

            // act
            var result =
                events.Aggregate<object, AttendantListReadModel>(null, When);

            // assert
            result.Attendants.Should().HaveCount(2);
            Attendant(alice).Should().BeNull();
            Attendant(joe).Waiting.Should().BeFalse();
            Attendant(carla).Waiting.Should().BeFalse();

            Attendant Attendant(Guid userId) =>
                result.Attendants.FirstOrDefault(x => x.UserId == userId);
        }
    }
}