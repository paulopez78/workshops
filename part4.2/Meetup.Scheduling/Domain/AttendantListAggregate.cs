using System;

namespace Meetup.Scheduling.Domain
{
    public class AttendantListAggregate : Aggregate
    {
        AttendantListAggregate()
        {
        }

        public AttendantList AttendantList { get; }

        public AttendantListAggregate(Guid id, PositiveNumber capacity)
        {
            Id            = id;
            AttendantList = AttendantList.From(capacity);
        }

        public void IncreaseCapacity(PositiveNumber number)
            => AttendantList.IncreaseCapacity(number);

        public void ReduceCapacity(PositiveNumber number)
            => AttendantList.ReduceCapacity(number);

        public void AcceptInvitation(Guid userId, DateTimeOffset acceptedAt)
            => AttendantList.Accept(userId, acceptedAt);

        public void DeclineInvitation(Guid userId, DateTimeOffset declinedAt)
            => AttendantList.Decline(userId, declinedAt);
    }
}