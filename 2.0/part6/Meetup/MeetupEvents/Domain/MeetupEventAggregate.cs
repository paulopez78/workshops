using System;
using static MeetupEvents.Contracts.MeetupEvents.V1;

namespace MeetupEvents.Domain
{
    public class MeetupEventAggregate : Aggregate
    {
        public Guid    GroupId           { get; private set; }
        public Details Details           { get; private set; }
        public string? CancelationReason { get; private set; }

        public ScheduleTime? ScheduleTime { get; private set; }

        public Location? Location { get; private set; }

        public MeetupEventStatus Status { get; private set; } = MeetupEventStatus.None;

        public void Create(Guid id, Guid groupId, Details details)
        {
            EnforceStatusBe(MeetupEventStatus.None);

            Id      = id;
            GroupId = groupId;
            Details = details;
            Status  = MeetupEventStatus.Draft;

            _changes.Add(new MeetupEventCreated(id));
        }

        public void MakeOnline(Uri url)
        {
            EnforceStatusBe(MeetupEventStatus.Draft);
            Location = Location.OnLine(url);

            _changes.Add(new MadeOnline(Id, url));
        }

        public void MakeOnsite(Address address)
        {
            EnforceStatusBe(MeetupEventStatus.Draft);
            Location = Location.OnSite(address);

            _changes.Add(new MadeOnsite(Id, address));
        }

        public void Schedule(ScheduleTime scheduleTime)
        {
            EnforceStatusBe(MeetupEventStatus.Draft);
            ScheduleTime = scheduleTime;

            _changes.Add(new Scheduled(Id, ScheduleTime.Start, ScheduleTime.End));
        }

        public void UpdateDetails(Details details)
        {
            EnforceStatusNotBe(MeetupEventStatus.Cancelled);

            Details = details;

            _changes.Add(new DetailsUpdated(Id, details.Title, details.Description));
        }

        public void Publish(DateTimeOffset at)
        {
            EnforceStatusBe(MeetupEventStatus.Draft);
            EnforceStatusNotBe(MeetupEventStatus.Cancelled);

            EnforceLocation();
            EnforceSchedule();

            Status = MeetupEventStatus.Published;

            _changes.Add(new Published(Id, at));
        }

        public void Cancel(string reason, DateTimeOffset at)
        {
            EnforceStatusBe(MeetupEventStatus.Published);

            Status            = MeetupEventStatus.Cancelled;
            CancelationReason = reason;

            _changes.Add(new Canceled(Id, reason, at));
        }

        void EnforceLocation()
        {
            if (Location is null)
                throw new ArgumentNullException(nameof(Location));
        }

        void EnforceSchedule()
        {
            if (Location is null)
                throw new ArgumentNullException(nameof(ScheduleTime));
        }

        void EnforceStatusBe(MeetupEventStatus status)
        {
            if (Status != status)
                throw new InvalidOperationException($"Meetup {Id} must be in {status}");
        }

        void EnforceStatusNotBe(MeetupEventStatus status)
        {
            if (Status == status)
                throw new InvalidOperationException($"Meetup {Id} must not be in {status}");
        }
    }

    public enum MeetupEventStatus
    {
        None,
        Draft,
        Published,
        Cancelled
    }
}