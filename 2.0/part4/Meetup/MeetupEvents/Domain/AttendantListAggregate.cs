using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetupEvents.Domain
{
    public class AttendantListAggregate : Aggregate
    {
        readonly List<Attendant>          _attendants = new();
        public   IReadOnlyList<Attendant> Attendants        => _attendants.OrderBy(x => x.At).ToList();
        public   IReadOnlyList<Attendant> OrderedAttendants => _attendants.OrderBy(x => x.At).ToList();
        public   Guid                     MeetupEventId     { get; private set; }
        public   PositiveNumber           Capacity          { get; private set; }
        public   AttendantListStatus      Status            { get; private set; } = AttendantListStatus.None;


        public void Create(Guid id, Guid meetupEventId, int capacity, int defaultCapacity = 50)
        {
            EnforceStatusBe(AttendantListStatus.None);

            Id            = id;
            Capacity      = capacity == 0 ? defaultCapacity : capacity;
            Status        = AttendantListStatus.Closed;
            MeetupEventId = meetupEventId;
        }

        public void Open()
        {
            EnforceStatusNotBe(AttendantListStatus.Opened);
            EnforceStatusNotBe(AttendantListStatus.Archived);

            Status = AttendantListStatus.Opened;
        }

        public void Close()
        {
            EnforceStatusNotBe(AttendantListStatus.Closed);
            EnforceStatusNotBe(AttendantListStatus.Archived);

            EnforceStatusBe(AttendantListStatus.Opened);

            Status = AttendantListStatus.Closed;
        }

        public void ReduceCapacity(PositiveNumber byNumber)
        {
            EnforceStatusNotBe(AttendantListStatus.Archived);

            Capacity -= byNumber;

            OrderedAttendants
                .Where(x => !x.Waiting)
                .TakeLast(byNumber)
                .ToList()
                .ForEach(x => x.Wait());
        }

        public void IncreaseCapacity(PositiveNumber byNumber)
        {
            EnforceStatusNotBe(AttendantListStatus.Archived);

            Capacity += byNumber;

            OrderedAttendants
                .Where(x => x.Waiting)
                .Take(byNumber)
                .ToList()
                .ForEach(x => x.Attend());
        }

        public void Attend(Guid memberId, DateTimeOffset at)
        {
            EnforceStatusBe(AttendantListStatus.Opened);

            var attendant = _attendants.FirstOrDefault(x => x.UserId == memberId);
            if (attendant is not null)
                throw new InvalidOperationException($"Member {memberId} already going to the meetup");

            if (HasFreeSpots())
                _attendants.Add(new(memberId, at));
            else
                _attendants.Add(new(memberId, at, waiting: true));

            bool HasFreeSpots() => Capacity - _attendants.Count > 0;
        }

        public void CancelAttendance(Guid memberId, DateTimeOffset at)
        {
            EnforceStatusBe(AttendantListStatus.Opened);

            _attendants.RemoveAll(x => x.UserId == memberId);

            // first in the waiting list moved to attend 
            OrderedAttendants.FirstOrDefault(x => x.Waiting)?.Attend();
        }

        void EnforceStatusBe(AttendantListStatus status)
        {
            if (Status != status)
                throw new InvalidOperationException($"Attendant list {Id} must be in {status}");
        }

        void EnforceStatusNotBe(AttendantListStatus status)
        {
            if (Status == status)
                throw new InvalidOperationException($"Attendant list {Id} must not be in {status}");
        }
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

        public void Wait()
            => Waiting = true;
    }

    public enum AttendantListStatus
    {
        None,
        Opened,
        Closed,
        Archived
    }
}