using System;
using System.Collections.Generic;
using System.Linq;
using Meetup.Scheduling.Framework;
using Meetup.Scheduling.Shared;
using static Meetup.Scheduling.AttendantList.Events.V1;

namespace Meetup.Scheduling.AttendantList
{
    public class AttendantListAggregate : Aggregate
    {
        readonly List<Attendant> Attendants = new();
        PositiveNumber           Capacity;
        AttendantListStatus      Status;
        Guid                     MeetupEventId;

        public void Create(Guid meetupEventId, PositiveNumber capacity)
            => Apply(new AttendantListCreated(Id, meetupEventId, capacity));

        public void Open()
        {
            EnforceNotArchived();

            if (Status == AttendantListStatus.Opened)
                throw new ApplicationException($"AttendantList {Id} already opened");

            Apply(new Opened(Id, MeetupEventId));
        }

        public void Close()
        {
            EnforceOpened();

            if (Status == AttendantListStatus.Closed)
                throw new ApplicationException($"AttendantList {Id} already closed");

            Apply(new Closed(Id, MeetupEventId));
        }

        public void IncreaseCapacity(PositiveNumber number, DateTimeOffset increasedAt)
        {
            EnforceNotArchived();

            Apply(new CapacityIncreased(Id, MeetupEventId, number));

            TryRemoveFromWaitingList();

            void TryRemoveFromWaitingList()
            {
                var shouldAttend = Waiting().Take(FreeSpots).ToArray();

                if (shouldAttend.Any())
                    Apply(new AttendantsRemovedFromWaitingList(Id, MeetupEventId, increasedAt, shouldAttend));

                IEnumerable<Guid> Waiting() => UserIds(waiting: true);
            }
        }


        public void ReduceCapacity(PositiveNumber number, DateTimeOffset reducedAt)
        {
            EnforceNotArchived();

            Apply(new CapacityReduced(Id, MeetupEventId, number));

            TryAddToWaitingList();

            void TryAddToWaitingList()
            {
                var shouldWait = Going().TakeLast(MissingSpots()).ToArray();

                if (shouldWait.Any())
                    Apply(new AttendantsAddedToWaitingList(Id, MeetupEventId, reducedAt, shouldWait));

                int MissingSpots() => Going().Count() - Capacity;

                IEnumerable<Guid> Going() => UserIds(waiting: false);
            }
        }

        public void Add(Guid userId, DateTimeOffset addedAt)
        {
            EnforceOpened();

            if (Attendants.Any(x => x.UserId == userId))
                throw new ApplicationException($"Attendant {userId} already added");

            if (FreeSpots > 0)
                Apply(new AttendantAdded(Id, MeetupEventId, userId, addedAt));
            else
                Apply(new AttendantWaitingAdded(Id, MeetupEventId, userId, addedAt));
        }

        public void Remove(Guid userId, DateTimeOffset removedAt)
        {
            EnforceOpened();

            if (Attendants.All(x => x.UserId != userId))
                throw new ApplicationException($"Attendant {userId} already removed");

            Apply(new AttendantRemoved(Id, MeetupEventId, userId, removedAt));

            TryRemoveFromWaitingList();

            void TryRemoveFromWaitingList()
            {
                var shouldAttend = OrderedAttendants.FirstOrDefault(x => x.Waiting);
                if (shouldAttend is not null)
                    Apply(new AttendantsRemovedFromWaitingList(Id, MeetupEventId, removedAt, shouldAttend.UserId));
            }
        }

        public bool Going(Guid userId)
            => Attendants.Any(x => x.UserId == userId && !x.Waiting);

        public bool Waiting(Guid userId)
            => Attendants.Any(x => x.UserId == userId && x.Waiting);

        public bool NotGoing(Guid userId)
            => Attendants.All(x => x.UserId != userId);

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

        public override void When(object domainEvent)
        {
            switch (domainEvent)
            {
                case AttendantListCreated created:
                    MeetupEventId = created.MeeupEventId;
                    Capacity      = created.Capacity;
                    Status        = AttendantListStatus.Closed;
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

        public record Attendant(Guid UserId, DateTimeOffset AddedAt, bool Waiting);
    }

    public enum AttendantListStatus
    {
        Closed,
        Opened,
        Archived
    }
}