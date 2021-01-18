using System;
using System.Collections.Generic;
using System.Linq;
using MassTransit.Contracts.Conductor;
using Meetup.Scheduling.Shared;
using static Meetup.Scheduling.AttendantList.Events.V1;

namespace Meetup.Scheduling.AttendantList
{
    public class AttendantListAggregate : Aggregate
    {
        public List<Attendant>     Attendants { get; } = new();
        public PositiveNumber      Capacity;
        public AttendantListStatus Status { get; private set; }

        public AttendantListAggregate()
        {
        }

        public void Create(PositiveNumber capacity)
            => Apply(new Created(Id, capacity));

        public void Open()
        {
            EnforceNotArchived();

            if (Status == AttendantListStatus.Opened)
                return;

            Apply(new Opened(Id));
        }

        public void Close()
        {
            EnforceOpened();

            if (Status == AttendantListStatus.Closed)
                return;

            Apply(new Closed(Id));
        }

        public void IncreaseCapacity(PositiveNumber number, DateTimeOffset increasedAt)
        {
            EnforceNotArchived();

            Apply(new CapacityIncreased(Id, number));

            TryRemoveFromWaitingList();

            void TryRemoveFromWaitingList()
            {
                var shouldAttend = Waiting().Take(FreeSpots).ToArray();

                if (shouldAttend.Any())
                    Apply(new AttendantsRemovedFromWaitingList(Id, increasedAt, shouldAttend));

                IEnumerable<Guid> Waiting() => UserIds(waiting: true);
            }
        }


        public void ReduceCapacity(PositiveNumber number, DateTimeOffset reducedAt)
        {
            EnforceNotArchived();

            Apply(new CapacityReduced(Id, number));

            TryAddToWaitingList();

            void TryAddToWaitingList()
            {
                var shouldWait = Going().TakeLast(MissingSpots()).ToArray();

                if (shouldWait.Any())
                    Apply(new AttendantsAddedToWaitingList(Id, shouldWait, reducedAt));

                int MissingSpots() => Going().Count() - Capacity;

                IEnumerable<Guid> Going() => UserIds(waiting: false);
            }
        }

        public void Add(Guid userId, DateTimeOffset addedAt)
        {
            EnforceOpened();

            if (Attendants.Any(x => x.UserId == userId))
                return;

            if (FreeSpots > 0)
                Apply(new AttendantAdded(Id, userId, addedAt));
            else
                Apply(new AttendantWaitingAdded(Id, userId, addedAt));
        }

        public void Remove(Guid userId, DateTimeOffset removedAt)
        {
            EnforceOpened();

            if (Attendants.All(x => x.UserId != userId))
                return;

            Apply(new AttendantRemoved(Id, userId, removedAt));

            TryRemoveFromWaitingList();

            void TryRemoveFromWaitingList()
            {
                var shouldAttend = OrderedAttendants.FirstOrDefault(x => x.Waiting);
                if (shouldAttend is not null)
                    Apply(new AttendantsRemovedFromWaitingList(Id, removedAt, shouldAttend.UserId));
            }
        }

        void EnforceOpened()
        {
            if (Status != AttendantListStatus.Opened)
                throw new ApplicationException("Attendant list is not opened");
        }

        void EnforceNotArchived()
        {
            if (Status == AttendantListStatus.Archived)
                throw new ApplicationException("Attendant list is finished");
        }

        protected override void When(object domainEvent)
        {
            switch (domainEvent)
            {
                case Created created:
                    Capacity = created.Capacity;
                    Status   = AttendantListStatus.Closed;
                    break;
                case Opened _:
                    Status = AttendantListStatus.Opened;
                    break;
                case Closed _:
                    Status = AttendantListStatus.Closed;
                    break;
                case CapacityReduced reduced:
                    Capacity = Capacity - reduced.ByNumber;
                    break;
                case CapacityIncreased increased:
                    Capacity = Capacity + increased.ByNumber;
                    break;
                case AttendantAdded added:
                    Attendants.Add(new Attendant(added.UserId, added.AddedAt, Waiting: false));
                    break;
                case AttendantWaitingAdded waitingAdded:
                    Attendants.Add(new Attendant(waitingAdded.UserId, waitingAdded.AddedAt, Waiting: true));
                    break;
                case AttendantRemoved removed:
                    Attendants.RemoveAll(x => x.UserId == removed.UserId);
                    break;
                case AttendantsAddedToWaitingList waiting:
                    UpdateAttendants(waiting: true, waiting.Attendants);
                    break;
                case AttendantsRemovedFromWaitingList going:
                    UpdateAttendants(waiting: false, going.Attendants);
                    break;
            }

            void UpdateAttendants(bool waiting, params Guid[] attendants)
            {
                foreach (var attendant in attendants)
                {
                    var found = Attendants.FirstOrDefault(x => x.UserId == attendant);
                    if (found is not null)
                    {
                        Attendants.Remove(found);
                        Attendants.Add(found with {Waiting = waiting});
                    }
                }
            }
        }

        IEnumerable<Attendant> OrderedAttendants => Attendants.OrderBy(x => x.AddedAt);

        IEnumerable<Guid> UserIds(bool waiting) =>
            OrderedAttendants.Where(x => x.Waiting == waiting).Select(x => x.UserId);

        PositiveNumber FreeSpots => Capacity - OrderedAttendants.Count(x => !x.Waiting);
    }

    public enum AttendantListStatus
    {
        Closed,
        Opened,
        Archived
    }

    public record Attendant(Guid UserId, DateTimeOffset AddedAt, bool Waiting);
}