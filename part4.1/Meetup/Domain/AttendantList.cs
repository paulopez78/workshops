using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.Scheduling.Domain
{
    public class AttendantList : Aggregate
    {
        public Guid MeetupEventId { get; }
        public int  Capacity      { get; private set; }

        readonly List<Attendant>                _attendants = new();
        public   IReadOnlyCollection<Attendant> Attendants => _attendants;

        IReadOnlyList<Attendant> Going   => Where(AttendantStatus.Going);
        IReadOnlyList<Attendant> Waiting => Where(AttendantStatus.Waiting);

        IReadOnlyList<Attendant> Where(AttendantStatus status) =>
            Attendants.Where(x => x.Status == status).OrderBy(x => x.ModifiedAt).ToList();

        int  FreeSpots    => Capacity - Going.Count;
        int  MissingSpots => !HasFreeSpots ? Going.Count - Capacity : 0;
        bool HasFreeSpots => FreeSpots > 0;

        public AttendantList(Guid id, Guid meetupEventId, int capacity)
        {
            Id            = id;
            MeetupEventId = meetupEventId;
            Capacity      = capacity;
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
            var invitation = GetOrAddInvitation(userId);

            if (HasFreeSpots)
                invitation.Accept(acceptedAt);
            else
                invitation.Wait(acceptedAt);
        }

        public void DeclineInvitation(Guid userId, DateTimeOffset declinedAt)
        {
            var invitation = GetOrAddInvitation(userId);
            invitation.Decline(declinedAt);

            UpdateAttendantsList();
        }

        void UpdateAttendantsList()
        {
            if (HasFreeSpots)
                Waiting.Take(FreeSpots).ToList().ForEach(x => x.Accept());
            else
                Going.TakeLast(MissingSpots).ToList().ForEach(x => x.Wait());
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

    public class Attendant : Entity
    {
        public Attendant(Guid userId)
        {
            UserId = userId;
            Status = AttendantStatus.Unknown;
        }

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