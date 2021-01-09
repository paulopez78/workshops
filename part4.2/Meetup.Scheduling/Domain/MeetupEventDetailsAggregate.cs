using System;

namespace Meetup.Scheduling.Domain
{
    public class MeetupEventDetailsAggregate : Aggregate
    {
        public string            Group  { get; }
        public string            Title  { get; private set; }
        public MeetupEventStatus Status { get; private set; } = MeetupEventStatus.Draft;

        public MeetupEventDetailsAggregate(Guid id, string group, string title)
        {
            Id    = id;
            Group = group;
            Title = title;
        }

        public void UpdateDetails(string title)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException(nameof(title));
            Title = title;
        }

        public void Publish()
        {
            if (Status == MeetupEventStatus.Draft)
                Status = MeetupEventStatus.Scheduled;
        }

        public void Cancel()
        {
            if (Status == MeetupEventStatus.Scheduled)
                Status = MeetupEventStatus.Cancelled;
        }
    }

    public enum MeetupEventStatus
    {
        Draft,
        Scheduled,
        Cancelled
    }
}