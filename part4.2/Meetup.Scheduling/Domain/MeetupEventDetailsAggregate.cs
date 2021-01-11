using System;

namespace Meetup.Scheduling.Domain
{
    public class MeetupEventDetailsAggregate : Aggregate
    {
        public GroupSlug         Group        { get; }
        public Details           Details      { get; private set; }
        public Location?         Location     { get; private set; }
        public DateTimeRange?    ScheduleTime { get; private set; }
        public MeetupEventStatus Status       { get; private set; } = MeetupEventStatus.Draft;

        MeetupEventDetailsAggregate()
        {
        }

        public MeetupEventDetailsAggregate(Guid id, GroupSlug group, Details details)
        {
            Id      = id;
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

        public void Schedule(DateTimeRange dateTimeRange)
        {
            // idempotent
            if (ScheduleTime == dateTimeRange)
                throw new ApplicationException("Same scheduled time");

            ScheduleTime = dateTimeRange;
            if (Status == MeetupEventStatus.Published)
            {
                // notify attendants
                // ScheduleChanged
            }
        }

        public void Publish()
        {
            // idempotent
            if (Status == MeetupEventStatus.Published)
                throw new ApplicationException("Already published");

            if (Status == MeetupEventStatus.Cancelled)
                throw new ApplicationException("Can not publish a cancelled event");

            if (ScheduleTime is null)
                throw new ApplicationException("Can not publish without scheduled time");

            if (Location is null)
                throw new ApplicationException("Can not publish without location");
        }

        public void Cancel()
        {
            // idempotent
            if (Status == MeetupEventStatus.Cancelled)
                throw new ApplicationException("Already cancelled");

            if (Status == MeetupEventStatus.Published)
                Status = MeetupEventStatus.Cancelled;
        }
    }

    public enum MeetupEventStatus
    {
        Draft,
        Published,
        Cancelled
    }
}