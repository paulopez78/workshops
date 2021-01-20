using System;
using System.Linq;
using Marten.Events.Projections;
using static Meetup.Scheduling.MeetupDetails.Events.V1;
using static Meetup.Scheduling.AttendantList.Events.V1;
using static System.Collections.Immutable.ImmutableList;

namespace Meetup.Scheduling.Queries
{
    public class MeetupGroupEventsProjection : ViewProjection<MeetupGroupEvents, Guid>
    {
        public MeetupGroupEventsProjection()
        {
        }
    }
}