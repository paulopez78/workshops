using System;
using static Meetup.Scheduling.Domain.Events.V1.MeetupEvent;

namespace Meetup.Scheduling.Domain
{
    public class MeetupEventDetailsAggregate : Aggregate
    {
        public GroupSlug         Group        { get; }
        public Details           Details      { get; private set; }
        public Location?         Location     { get; private set; }
        public ScheduleDateTime? ScheduleTime { get; private set; }
        public MeetupEventStatus Status       { get; private set; } = MeetupEventStatus.Draft;

        MeetupEventDetailsAggregate()
        {
        }

        public MeetupEventDetailsAggregate(Guid id, GroupSlug group, Details details, PositiveNumber capacity)
        {
            Id      = id;
            Group   = group;
            Details = details;

            Events.Add(new Created(Id, group, details.Title, details.Description, capacity));
        }

        public void UpdateDetails(Details details)
        {
            if (Details == details)
                throw new ApplicationException("Same details");

            Details = details;

            Events.Add(new DetailsUpdated(Id, details.Title, details.Description));
        }

        public void MakeOnlineEvent(Uri url)
        {
            var newLocation = Location.Online(url);

            if (Location == newLocation)
                throw new ApplicationException("Same location");

            Location = newLocation;

            Events.Add(new MadeOnline(Id, url.ToString()));
        }

        public void MakeOnSiteEvent(Address address)
        {
            var newLocation = Location.OnSite(address);

            if (Location == newLocation)
                throw new ApplicationException("Same location");

            Location = newLocation;

            Events.Add(new MadeOnsite(Id, address));
        }

        public void Schedule(ScheduleDateTime scheduleTime)
        {
            // idempotent
            if (ScheduleTime == scheduleTime)
                throw new ApplicationException("Same scheduled time");

            ScheduleTime = scheduleTime;

            Events.Add(new Scheduled(Id, scheduleTime.Start, scheduleTime.End));
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

            Events.Add(new Published(Id));
        }

        public void Cancel(string reason)
        {
            Status = Status switch
            {
                MeetupEventStatus.Cancelled => throw new ApplicationException("Already cancelled"),
                MeetupEventStatus.Published => MeetupEventStatus.Cancelled,
                _                           => Status
            };

            Events.Add(new Cancelled(Id, reason));
        }

        public void Start()
        {
            Status = MeetupEventStatus.Started;
            Events.Add(new Started(Id));
        }

        public void Finish()
        {
            Status = MeetupEventStatus.Finished;
            Events.Add(new Finished(Id));
        }
    }

    public enum MeetupEventStatus
    {
        Draft,
        Published,
        Cancelled,
        Started,
        Finished
    }
}