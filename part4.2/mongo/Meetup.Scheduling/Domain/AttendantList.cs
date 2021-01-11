using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.Scheduling.Domain
{
    public record AttendantList
    {
        public PositiveNumber Capacity { get; private set; }

        public List<Attendant> Attendants { get; init; } = new();

        AttendantList(PositiveNumber capacity) => Capacity = capacity;

        public static AttendantList From(PositiveNumber capacity) => new(capacity);

        public void Accept(Guid userId, DateTimeOffset acceptedAt)
        {
            if (AlreadyGoing(userId)) return;

            var attendant = TryAddAttendant(userId);

            if (HasFreeSpots)
                attendant.Going(acceptedAt);
            else
                attendant.Wait(acceptedAt);
        }

        public void Decline(Guid userId, DateTimeOffset declinedAt)
        {
            if (AlreadyNotGoing(userId)) return;

            var attendant = TryAddAttendant(userId);

            attendant.NotGoing(declinedAt);

            UpdateAttendantsStatus();
        }

        public void IncreaseCapacity(PositiveNumber number)
        {
            Capacity = Capacity + number;
            UpdateAttendantsStatus();
        }

        public void ReduceCapacity(PositiveNumber number)
        {
            Capacity = Capacity - number;
            UpdateAttendantsStatus();
        }

        public AttendantStatus? Status(Guid userId) =>
            Attendants.FirstOrDefault(x => x.UserId == userId)?.Status;

        IReadOnlyList<Attendant> Going   => Where(AttendantStatus.Going);
        IReadOnlyList<Attendant> Waiting => Where(AttendantStatus.Waiting);

        IReadOnlyList<Attendant> Where(AttendantStatus status) =>
            Attendants.Where(x => x.Status == status).OrderBy(x => x.ModifiedAt).ToList();

        PositiveNumber FreeSpots    => Capacity - Going.Count;
        PositiveNumber MissingSpots => Going.Count - Capacity;

        bool HasFreeSpots => FreeSpots > 0;
        bool AlreadyGoing(Guid userId) => Already(userId, AttendantStatus.Going);
        bool AlreadyNotGoing(Guid userId) => Already(userId, AttendantStatus.NotGoing);

        bool Already(Guid userId, AttendantStatus status) =>
            Attendants.Any(x => x.UserId == userId && x.Status == status);

        Attendant TryAddAttendant(Guid userId)
        {
            var attendant = Attendants.FirstOrDefault(x => x.UserId == userId);
            if (attendant is not null) return attendant;

            attendant = new Attendant(userId);
            Attendants.Add(attendant);
            return attendant;
        }

        void UpdateAttendantsStatus()
        {
            if (HasFreeSpots)
                Waiting.Take(FreeSpots).ToList().ForEach(x => x.MoveToGoing());
            else
                Going.TakeLast(MissingSpots).ToList().ForEach(x => x.MoveToWaiting());
        }
    }

    public record Attendant
    {
        public Guid            UserId     { get; init; }
        public AttendantStatus Status     { get; private set; } = AttendantStatus.Unknown;
        public DateTimeOffset  ModifiedAt { get; private set; }

        public Attendant(Guid userId) => UserId = userId;

        public void Going(DateTimeOffset at) => Update(AttendantStatus.Going, at);
        public void NotGoing(DateTimeOffset at) => Update(AttendantStatus.NotGoing, at);
        public void Wait(DateTimeOffset at) => Update(AttendantStatus.Waiting, at);
        public void MoveToGoing() => Status = AttendantStatus.Going;
        public void MoveToWaiting() => Status = AttendantStatus.Waiting;

        void Update(AttendantStatus status, DateTimeOffset at)
        {
            Status     = status;
            ModifiedAt = at;
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