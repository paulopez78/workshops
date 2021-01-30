using System;

namespace MeetupEvents.Domain
{
    public class MeetupEventAggregate : Aggregate
    {
        public Guid              GroupId           { get; private set; }
        public string            Title             { get; private set; }
        public string            Description       { get; private set; }
        public string?           CancelationReason { get; private set; }
        public MeetupEventStatus Status            { get; private set; } = MeetupEventStatus.None;


        public void Create(Guid id, Guid groupId, string title, string description)
        {
            EnforceStatusBe(MeetupEventStatus.None);

            Id          = id;
            GroupId     = groupId;
            Title       = title;
            Description = description;
            Status      = MeetupEventStatus.Draft;
        }

        public void UpdateDetails(string title, string description)
        {
            EnforceStatusNotBe(MeetupEventStatus.Cancelled);

            Title       = title;
            Description = description;
        }

        public void Publish()
        {
            EnforceStatusBe(MeetupEventStatus.Draft);
            EnforceStatusNotBe(MeetupEventStatus.Cancelled);

            Status = MeetupEventStatus.Published;
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