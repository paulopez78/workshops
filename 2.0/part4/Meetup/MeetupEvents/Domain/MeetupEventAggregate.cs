using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetupEvents.Domain
{
    public class MeetupEventAggregate
    {
        public Guid              Id                { get; private set; }
        public int               Version           { get; private set; }
        public Guid              GroupId           { get; private set; }
        public string            Title             { get; private set; }
        public string            Description       { get; private set; }
        public int               Capacity          { get; private set; }
        public string?           CancelationReason { get; private set; }
        public MeetupEventStatus Status            { get; private set; } = MeetupEventStatus.None;

        readonly List<Attendant>          _attendants = new();
        public   IReadOnlyList<Attendant> Attendants => _attendants;

        public void Create(Guid id, Guid groupId, string title, string description, int capacity,
            int defaultCapacity = 50)
        {
            EnforceStatusBe(MeetupEventStatus.None);

            Id          = id;
            GroupId     = groupId;
            Title       = title;
            Description = description;
            Capacity    = capacity == 0 ? defaultCapacity : capacity;
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

        public void Attend(Guid memberId, DateTimeOffset at)
        {
            EnforceStatusBe(MeetupEventStatus.Published);

            var attendant = _attendants.FirstOrDefault(x => x.UserId == memberId);
            if (attendant is not null)
                throw new InvalidOperationException($"Member {memberId} already going to the meetup");

            if (HasFreeSpots())
                _attendants.Add(new(memberId, at));
            else
                _attendants.Add(new(memberId, at, waiting: true));

            bool HasFreeSpots() => Capacity > _attendants.Count;
        }

        public void CancelAttendance(Guid memberId, DateTimeOffset at)
        {
            EnforceStatusBe(MeetupEventStatus.Published);

            _attendants.RemoveAll(x => x.UserId == memberId);

            var waiting = _attendants.Where(x => x.Waiting).OrderBy(x => x.At).FirstOrDefault();
            waiting?.Attend();
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

        public void IncreaseVersion() => Version += 1;
    }

    public class Attendant
    {
        public Guid           Id      { get; init; }
        public Guid           UserId  { get; private set; }
        public bool           Waiting { get; private set; }
        public DateTimeOffset At      { get; private set; }

        public Attendant(Guid userId, DateTimeOffset at, bool waiting = false)
        {
            UserId  = userId;
            At      = at;
            Waiting = waiting;
        }

        public void Attend()
            => Waiting = false;
    }

    public enum MeetupEventStatus
    {
        None,
        Draft,
        Published,
        Cancelled
    }
}