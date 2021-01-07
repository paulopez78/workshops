using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.Scheduling.Domain
{
    public class MeetupEvent
    {
        public Guid   Id       { get; }
        public string Group    { get; }
        public string Title    { get; private set; }
        public int    Capacity { get; private set; }

        public int Version { get; private set; } = -1;

        public MeetupEventStatus Status { get; private set; } = MeetupEventStatus.Draft;

        readonly List<Attendant>                _attendants = new();
        public   IReadOnlyCollection<Attendant> Attendants => _attendants;

        IReadOnlyList<Attendant> Going => Attendants.Where(x => x.Status == AttendantStatus.Going)
            .OrderBy(x => x.ModifiedAt).ToList();

        IReadOnlyList<Attendant> Waiting => Attendants.Where(x => x.Status == AttendantStatus.Waiting)
            .OrderBy(x => x.ModifiedAt).ToList();

        int  FreeSpots    => Capacity - Going.Count;
        bool HasFreeSpots => FreeSpots > 0;

        public MeetupEvent(Guid id, string group, string title, int capacity)
        {
            Id       = id;
            Group    = group;
            Title    = title;
            Capacity = capacity;
        }

        public void IncreaseVersion() => Version += 1;

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

        public void IncreaseCapacity(int increase)
        {
            Capacity += increase;
            UpdateAttendantsList();
        }

        public void ReduceCapacity(int decrease)
        {
            var newCapacity = Capacity - decrease;
            Capacity = newCapacity > 0 ? newCapacity : 0;
            UpdateAttendantsList();
        }

        public void AcceptInvitation(Guid userId, DateTimeOffset acceptedAt)
        {
            EnforceScheduledStatus();

            var invitation = GetOrAddInvitation(userId);

            if (HasFreeSpots)
                invitation.Accept(acceptedAt);
            else
                invitation.Wait(acceptedAt);
        }

        public void DeclineInvitation(Guid userId, DateTimeOffset declinedAt)
        {
            EnforceScheduledStatus();

            var invitation = GetOrAddInvitation(userId);
            invitation.Decline(declinedAt);

            UpdateAttendantsList();
        }

        void UpdateAttendantsList()
        {
            if (HasFreeSpots)
            {
                Waiting.Take(FreeSpots).ToList().ForEach(x => x.Accept());
            }
            else
            {
                var reducedCapacity = Going.Count - Capacity;
                Going.TakeLast(reducedCapacity).ToList().ForEach(x => x.Wait());
            }
        }

        void EnforceScheduledStatus()
        {
            if (Status is not MeetupEventStatus.Scheduled)
                throw new ApplicationException("Meetup not scheduled");
        }

        Attendant GetOrAddInvitation(Guid userId)
        {
            var invitation = Attendants.FirstOrDefault(x => x.UserId == userId);
            if (invitation is not null) return invitation;

            invitation = new Attendant(userId);
            _attendants.Add(invitation);

            return invitation;
        }
    }

    public enum MeetupEventStatus
    {
        Draft,
        Scheduled,
        Cancelled
    }

    public class Attendant
    {
        public Attendant(Guid userId)
        {
            UserId = userId;
            Status = AttendantStatus.Unknown;
        }

        public Guid Id { get; }

        public Guid UserId { get; }

        public AttendantStatus Status { get; private set; }

        public DateTimeOffset ModifiedAt { get; private set; }

        public void Accept() =>
            Status = AttendantStatus.Going;

        public void Accept(DateTimeOffset acceptedAt)
        {
            ModifiedAt = acceptedAt;
            Status     = AttendantStatus.Going;
        }

        public void Wait() =>
            Status = AttendantStatus.Waiting;

        public void Wait(DateTimeOffset waitingAt)
        {
            ModifiedAt = waitingAt;
            Status     = AttendantStatus.Waiting;
        }

        public void Decline(DateTimeOffset declinedAt)
        {
            ModifiedAt = declinedAt;
            Status     = AttendantStatus.NotGoing;
        }
    }

    public enum AttendantStatus
    {
        Unknown,
        Going,
        NotGoing,
        Waiting
    }
}