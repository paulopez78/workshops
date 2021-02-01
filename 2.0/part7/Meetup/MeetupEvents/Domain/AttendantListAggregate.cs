using System;
using System.Collections.Generic;
using System.Linq;
using static MeetupEvents.Contracts.AttendantListEvents.V1;

namespace MeetupEvents.Domain
{
    public class AttendantListAggregate : Aggregate
    {
        readonly List<Attendant>          _attendants = new();
        public   IReadOnlyList<Attendant> Attendants        => _attendants;
        public   IEnumerable<Attendant>   OrderedAttendants => _attendants.OrderBy(x => x.At).ToList();
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

            _changes.Add(new AttendantListCreated(Id, MeetupEventId, Capacity));
        }

        public void Open(DateTimeOffset at)
        {
            EnforceStatusNotBe(AttendantListStatus.Opened);
            EnforceStatusNotBe(AttendantListStatus.Archived);

            Status = AttendantListStatus.Opened;

            _changes.Add(new Opened(Id, at));
        }

        public void Close(DateTimeOffset at)
        {
            EnforceStatusNotBe(AttendantListStatus.Closed);
            EnforceStatusNotBe(AttendantListStatus.Archived);

            EnforceStatusBe(AttendantListStatus.Opened);

            Status = AttendantListStatus.Closed;

            _changes.Add(new Closed(Id, at));
        }

        public void ReduceCapacity(PositiveNumber byNumber)
        {
            EnforceStatusNotBe(AttendantListStatus.Archived);

            Capacity -= byNumber;

            var shouldWait = OrderedAttendants
                .Where(x => !x.Waiting)
                .TakeLast(byNumber)
                .ToList();

            shouldWait.ForEach(x => x.Wait());

            _changes.AddRange(
                shouldWait.Select(x => new AttendantMovedToWaiting(Id, x.UserId, x.At))
            );
        }

        public void IncreaseCapacity(PositiveNumber byNumber)
        {
            EnforceStatusNotBe(AttendantListStatus.Archived);

            Capacity += byNumber;

            var shouldAttend = OrderedAttendants
                .Where(x => x.Waiting)
                .Take(byNumber)
                .ToList();

            shouldAttend.ForEach(x => x.Attend());

            _changes.AddRange(
                shouldAttend.Select(x => new AttendantAdded(Id, x.UserId, x.At))
            );
        }

        public void Attend(Guid memberId, DateTimeOffset at)
        {
            EnforceStatusBe(AttendantListStatus.Opened);

            var attendant = _attendants.FirstOrDefault(x => x.UserId == memberId);
            if (attendant is not null)
                throw new InvalidOperationException($"Member {memberId} already going to the meetup");

            if (HasFreeSpots())
            {
                _attendants.Add(new(memberId, at));
                _changes.Add(new AttendantAdded(Id, memberId, at));
            }
            else
            {
                _attendants.Add(new(memberId, at, waiting: true));
                _changes.Add(new AttendantMovedToWaiting(Id, memberId, at));
            }

            bool HasFreeSpots() => Capacity - _attendants.Count > 0;
        }

        public void CancelAttendance(Guid memberId, DateTimeOffset at)
        {
            EnforceStatusBe(AttendantListStatus.Opened);

            var attendant = _attendants.FirstOrDefault(x => x.UserId == memberId);
            if (attendant is null)
                throw new InvalidOperationException($"Member {memberId} already not going to the meetup");

            _attendants.Remove(attendant);

            var firstWaiting = OrderedAttendants.FirstOrDefault(x => x.Waiting);
            if (firstWaiting is null) return;

            firstWaiting.Attend();

            _changes.Add(new AttendantRemoved(Id, attendant.UserId, attendant.At));
            _changes.Add(new AttendantAdded(Id, firstWaiting.UserId, firstWaiting.At));
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