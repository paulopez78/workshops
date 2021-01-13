using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.Scheduling.Domain
{
    public record AttendantList
    {
        public   PositiveNumber                 Capacity { get; private set; }
        readonly List<Attendant>                _attendants = new();
        public   IReadOnlyCollection<Attendant> Attendants => _attendants;

        AttendantList()
        {
        }

        AttendantList(PositiveNumber capacity) => Capacity = capacity;

        public static AttendantList From(PositiveNumber capacity) => new(capacity);

        public Attendant Accept(Guid userId, DateTimeOffset acceptedAt)
        {
            if (AlreadyGoing(userId)) return Attendants.Single(x => x.UserId == userId);

            var attendant = TryAddAttendant(userId);

            if (HasFreeSpots)
                attendant.Attend(acceptedAt);
            else
                attendant.Wait(acceptedAt);

            return attendant;
        }

        public (Attendant, UpdatedAttendants) Decline(Guid userId, DateTimeOffset declinedAt)
        {
            if (AlreadyNotGoing(userId))
                return (Attendants.Single(x => x.UserId == userId), new UpdatedAttendants(new(), new()));

            var attendant = TryAddAttendant(userId);

            attendant.DontAttend(declinedAt);

            return (attendant, UpdateAttendantsStatus());
        }

        public UpdatedAttendants IncreaseCapacity(PositiveNumber number)
        {
            Capacity = Capacity + number;
            return UpdateAttendantsStatus();
        }

        public UpdatedAttendants ReduceCapacity(PositiveNumber number)
        {
            Capacity = Capacity - number;
            return UpdateAttendantsStatus();
        }

        public AttendantStatus? Status(Guid userId) =>
            Attendants.FirstOrDefault(x => x.UserId == userId)?.Status;

        IReadOnlyList<Attendant> Going => Where(AttendantStatus.Going);

        IReadOnlyList<Attendant> Waiting => Where(AttendantStatus.Waiting);

        IReadOnlyList<Attendant> Where(AttendantStatus status) =>
            Attendants.Where(x => x.Status == status).OrderBy(x => x.ModifiedAt).ToList();

        PositiveNumber FreeSpots    => Capacity - Going.Count;
        PositiveNumber MissingSpots => Going.Count - Capacity;

        bool HasFreeSpots => FreeSpots > 0;
        bool AlreadyGoing(Guid userId) => Already(userId, AttendantStatus.Going);
        bool AlreadyNotGoing(Guid userId) => Already(userId, AttendantStatus.NotGoing);

        bool Already(Guid userId, AttendantStatus status) =>
            _attendants.Any(x => x.UserId == userId && x.Status == status);

        Attendant TryAddAttendant(Guid userId)
        {
            var attendant = _attendants.FirstOrDefault(x => x.UserId == userId);
            if (attendant is not null) return attendant;

            attendant = new Attendant(userId);
            _attendants.Add(attendant);
            return attendant;
        }

        UpdatedAttendants UpdateAttendantsStatus()
        {
            var shouldGo = Waiting.Take(FreeSpots).ToList();
            shouldGo.ForEach(x => x.MoveToGoing());

            var shouldWait = Going.TakeLast(MissingSpots).ToList();
            shouldWait.ForEach(x => x.MoveToWaiting());

            return new UpdatedAttendants(shouldGo, shouldWait);
        }
    }


    public record Attendant
    {
        public Guid            UserId     { get; }
        public AttendantStatus Status     { get; private set; } = AttendantStatus.Unknown;
        public DateTimeOffset  ModifiedAt { get; private set; }

        public Attendant(Guid userId) => UserId = userId;

        public void Attend(DateTimeOffset at) => Update(AttendantStatus.Going, at);
        public void DontAttend(DateTimeOffset at) => Update(AttendantStatus.NotGoing, at);
        public void Wait(DateTimeOffset at) => Update(AttendantStatus.Waiting, at);

        public void MoveToGoing()
        {
            Status = AttendantStatus.Going;
        }

        public void MoveToWaiting()
        {
            Status = AttendantStatus.Waiting;
        }

        public bool Waiting => Status == AttendantStatus.Waiting;
        public bool Going   => Status == AttendantStatus.Going;

        void Update(AttendantStatus status, DateTimeOffset at)
        {
            Status     = status;
            ModifiedAt = at;
        }
    }

    public record UpdatedAttendants(List<Attendant> MoveToGoing, List<Attendant> MovedToWaiting);

    public enum AttendantStatus
    {
        Unknown,
        Going,
        NotGoing,
        Waiting
    }
}