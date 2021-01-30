using System;

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
        }

        public void MakeOnline(Uri url)
        {
            EnforceStatusBe(MeetupEventStatus.Draft);
            Location = Location.OnLine(url);
        }

        public void MakeOnsite(Address address)
        {
            EnforceStatusBe(MeetupEventStatus.Draft);
            Location = Location.OnSite(address);
        }

        public void Schedule(ScheduleTime scheduleTime)
        {
            EnforceStatusBe(MeetupEventStatus.Draft);
            ScheduleTime = scheduleTime;
        }

        public void UpdateDetails(Details details)
        {
            EnforceStatusNotBe(MeetupEventStatus.Cancelled);

            Details = details;
        }

        public void Publish()
        {
            EnforceStatusBe(MeetupEventStatus.Draft);
            EnforceStatusNotBe(MeetupEventStatus.Cancelled);
            
            EnforceLocation();
            EnforceSchedule();

            Status = MeetupEventStatus.Published;
        }

        private void EnforceLocation()
        {
            if (Location is null)
                throw new ArgumentNullException(nameof(Location));
        }
        
        private void EnforceSchedule()
        {
            if (Location is null)
                throw new ArgumentNullException(nameof(ScheduleTime));
        }

        public void Cancel(string reason)
        {
            EnforceStatusBe(MeetupEventStatus.Published);

            Status            = MeetupEventStatus.Cancelled;
            CancelationReason = reason;
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