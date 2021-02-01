using System;
using Meetup.Scheduling.Shared;
using static Meetup.Scheduling.MeetupDetails.Events.V1;

namespace Meetup.Scheduling.MeetupDetails
{
    public class MeetupEventDetailsAggregate : Aggregate
    {
        public GroupSlug         Group        { get; private set; }
        public Details           Details      { get; private set; }
        public Location?         Location     { get; private set; } = Location.None;
        public ScheduleDateTime? ScheduleTime { get; private set; } = ScheduleDateTime.None;
        public MeetupEventStatus Status       { get; private set; } = MeetupEventStatus.Draft;

        public MeetupEventDetailsAggregate()
        {
        }

        public void Create(GroupSlug group, Details details, PositiveNumber capacity)
            => Apply(new Created(Id, @group, details.Title, details.Description, capacity));

        public void UpdateDetails(Details details)
        {
            if (Details == details)
                throw new ApplicationException("Same details");

            Apply(new Events.V1.DetailsUpdated(Id, details.Title, details.Description));
        }

        public void MakeOnlineEvent(Uri url)
        {
            if (Location == Location.Online(url))
                throw new ApplicationException("Same location");

            Apply(new Events.V1.MadeOnline(Id, url.ToString()));
        }

        public void MakeOnSiteEvent(Address address)
        {
            if (Location == Location.OnSite(address))
                throw new ApplicationException("Same location");

            Apply(new Events.V1.MadeOnsite(Id, address));
        }

        public void Schedule(ScheduleDateTime scheduleTime)
        {
            if (ScheduleTime == scheduleTime)
                throw new ApplicationException("Same scheduled time");

            Apply(new Events.V1.Scheduled(Id, scheduleTime.Start, scheduleTime.End));
        }

        public void Publish()
        {
            if (Status == MeetupEventStatus.Published)
                throw new ApplicationException("Already published");

            if (Status == MeetupEventStatus.Cancelled)
                throw new ApplicationException("Meetup cancelled can not be published");

            if (ScheduleTime == ScheduleDateTime.None)
                throw new ApplicationException("Meetup can not be published without schedule");

            if (Location == Location.None)
                throw new ApplicationException("Meetup can not be published without location");

            Apply(new Events.V1.Published(Id));
        }

        public void Cancel(string reason)
        {
            if (Status == MeetupEventStatus.Cancelled)
                throw new ApplicationException("Already cancelled");

            Apply(new Events.V1.Cancelled(Id, reason));
        }

        public void Start()
        {
            if (Status == MeetupEventStatus.Started)
                throw new ApplicationException("Already started");

            Apply(new Events.V1.Started(Id));
        }

        public void Finish()
        {
            if (Status == MeetupEventStatus.Finished)
                throw new ApplicationException("Already started");

            Apply(new Events.V1.Finished(Id));
        }

        protected override void When(object domainEvent)
        {
            switch (domainEvent)
            {
                case Created created:
                    Group   = GroupSlug.From(created.Group);
                    Details = Details.From(created.Title, created.Description);
                    break;
                case Events.V1.DetailsUpdated details:
                    Details = Details.From(details.Title, details.Description);
                    break;
                case Events.V1.Scheduled schedule:
                    ScheduleTime = ScheduleDateTime.FromUnsafe(schedule.Start, schedule.End);
                    break;
                case Events.V1.MadeOnline online:
                    Location = Location.Online(new Uri(online.Url));
                    break;
                case Events.V1.MadeOnsite onsite:
                    Location = Location.OnSite(onsite.Address);
                    break;
                case Events.V1.Published _:
                    Status = MeetupEventStatus.Published;
                    break;
                case Events.V1.Cancelled _:
                    Status = MeetupEventStatus.Cancelled;
                    break;
                case Events.V1.Started _:
                    Status = MeetupEventStatus.Started;
                    break;
                case Events.V1.Finished _:
                    Status = MeetupEventStatus.Finished;
                    break;
            }
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