using System;
using System.Linq;
using static Meetup.Scheduling.Domain.Events.V1.AttendantList;

namespace Meetup.Scheduling.Domain
{
    public class AttendantListAggregate : Aggregate
    {
        AttendantListAggregate()
        {
        }

        public AttendantList       AttendantList { get; }
        public AttendantListStatus Status        { get; private set; }

        public AttendantListAggregate(Guid id, PositiveNumber capacity)
        {
            Id            = id;
            AttendantList = AttendantList.From(capacity);
            Status        = AttendantListStatus.Closed;

            Events.Add(new Created(Id, capacity));
        }

        public void Open()
        {
            EnforceNotArchived();

            if (Status == AttendantListStatus.Opened)
                return;

            Status = AttendantListStatus.Opened;
            Events.Add(new Opened(Id));
        }

        public void Close()
        {
            EnforceOpened();

            if (Status == AttendantListStatus.Closed)
                return;

            Status = AttendantListStatus.Closed;
            Events.Add(new Closed(Id));
        }

        public void IncreaseCapacity(PositiveNumber number)
        {
            EnforceNotArchived();
            var updatedAttendants = AttendantList.IncreaseCapacity(number);

            Events.Add(new CapacityIncreased(Id, number));

            AddAttendantsMovedEvents(updatedAttendants);
        }

        public void ReduceCapacity(PositiveNumber number)
        {
            EnforceNotArchived();

            var updatedAttendants = AttendantList.ReduceCapacity(number);

            Events.Add(new CapacityReduced(Id, number));

            AddAttendantsMovedEvents(updatedAttendants);
        }

        public void AcceptInvitation(Guid userId, DateTimeOffset acceptedAt)
        {
            EnforceOpened();

            var attendant = AttendantList.Accept(userId, acceptedAt);

            if (attendant.Going)
                Events.Add(new AttendantMovedToGoingList(Map(attendant)));

            if (attendant.Waiting)
                Events.Add(new AttendantMovedToWaitingList(Map(attendant)));
        }

        public void DeclineInvitation(Guid userId, DateTimeOffset declinedAt)
        {
            EnforceOpened();

            var (attendant, updatedAttendants) = AttendantList.Decline(userId, declinedAt);

            Events.Add(new AttendantMovedToNotGoingList(Map(attendant)));

            AddAttendantsMovedEvents(updatedAttendants);
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

        void AddAttendantsMovedEvents(UpdatedAttendants updatedAttendants)
        {
            Events.AddRange(
                updatedAttendants.MovedToWaiting.Select(x => new AttendantMovedToWaitingList(Map(x)))
            );
            Events.AddRange(
                updatedAttendants.MoveToGoing.Select(x => new AttendantMovedToGoingList(Map(x)))
            );
        }

        Events.V1.AttendantList.Attendant Map(Attendant attendant) =>
            new(Id, attendant.UserId, attendant.ModifiedAt);
    }

    public enum AttendantListStatus
    {
        Closed,
        Opened,
        Archived
    }
}