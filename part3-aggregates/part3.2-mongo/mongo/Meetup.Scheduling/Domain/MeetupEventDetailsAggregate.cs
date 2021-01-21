using System;

namespace Meetup.Scheduling.Domain
{
    public class MeetupEventDetailsAggregate : Aggregate
    {
        public GroupSlug         Group        { get; init; }
        public Details           Details      { get; private set; }
        public Location?         Location     { get; private set; }
        public ScheduleDateTime? ScheduleTime { get; private set; }
        public MeetupEventStatus Status       { get; private set; } = MeetupEventStatus.Draft;

        public MeetupEventDetailsAggregate(Guid id, GroupSlug group, Details details)
        {
            Id      = id.ToString();
            Group   = group;
            Details = details;
        }

        public void UpdateDetails(Details details)
        {
            if (Details == details)
                throw new ApplicationException("Same details");

            Details = details;
        }

        public void MakeOnlineEvent(Uri url)
        {
            var newLocation = Location.Online(url);

            if (Location == newLocation)
                throw new ApplicationException("Same location");

            Location = newLocation;
        }

        public void MakeOnSiteEvent(Address address)
        {
            var newLocation = Location.OnSite(address);

            if (Location == newLocation)
                throw new ApplicationException("Same location");

            Location = newLocation;
        }

        public void Schedule(ScheduleDateTime scheduleTime)
        {
            // idempotent
            if (ScheduleTime == scheduleTime)
                throw new ApplicationException("Same scheduled time");

            ScheduleTime = scheduleTime;

            if (Status == MeetupEventStatus.Published)
            {
                // notify attendants
                // ScheduleChanged
            }
        }

        public void Publish()
        {
            Status = (Status, ScheduleTime, Location) switch
            {
                (MeetupEventStatus.Published, _, _) => throw new ApplicationException("Already published"),
                (MeetupEventStatus.Cancelled, _, _) => throw new ApplicationException("Already cancelled"),

                (_, null, _) => throw new ApplicationException("Can not publish without scheduled time"),
                (_, _, null) => throw new ApplicationException("Can not publish without location"),
                _            => MeetupEventStatus.Published
            };
        }

        public void Cancel()
        {
            Status = Status switch
            {
                MeetupEventStatus.Cancelled => throw new ApplicationException("Already cancelled"),
                MeetupEventStatus.Published => MeetupEventStatus.Cancelled,
                _                           => Status
            };
        }
    }

    public enum MeetupEventStatus
    {
        Draft,
        Published,
        Cancelled
    }
}