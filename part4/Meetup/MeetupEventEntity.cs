using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.Scheduling.Domain
{
    public class MeetupEvent
    {
        public Guid Id { get; }
        public string Group { get; }
        public string Title { get; }
        public int Capacity { get; private set; }

        public MeetupEventStatus Status { get; private set; } = MeetupEventStatus.Draft;

        private readonly List<Invitation> _invitations = new();
        public IReadOnlyCollection<Invitation> Invitations => _invitations;

        public MeetupEvent(Guid id, string group, string title, int capacity)
        {
            Id = id;
            Group = group;
            Title = title;
            Capacity = capacity;
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
            => Capacity += increase;

        public void ReduceCapacity(int decrease)
        {
            var newCapacity = Capacity - decrease;
            Capacity = newCapacity < 1 ? 0 : newCapacity;
        }

        public void AcceptInvitation(Guid userId, DateTimeOffset acceptedAt)
        {
            EnforceScheduledStatus();

            if (NotEnoughCapacity())
                throw new ApplicationException("Meetup event doesnt have enough capacity");

            var invitation = TryAddInvitation(userId);
            invitation.Accept(acceptedAt);
        }


        public void DeclineInvitation(Guid userId)
        {
            EnforceScheduledStatus();
            
            var invitation = TryAddInvitation(userId);
            invitation.Decline();
        }
        
        void EnforceScheduledStatus()
        {
            if (Status is not MeetupEventStatus.Scheduled)
                throw new ApplicationException("Meetup not scheduled");
        }
        
        Invitation TryAddInvitation(Guid userId)
        {
            var invitation = Invitations.FirstOrDefault(x => x.UserId == userId);
            if (invitation is not null) return invitation;
            
            invitation = new Invitation(userId);
            _invitations.Add(invitation);

            return invitation;
        }

        bool NotEnoughCapacity() => Invitations.Count(x => x.Going) >= Capacity;
    }

    public enum MeetupEventStatus
    {
        Draft,
        Scheduled,
        Cancelled
    }

    public class Invitation
    {
        public Invitation(Guid userId)
        {
            UserId = userId;
        }

        public Guid Id { get; }

        public Guid UserId { get; }

        public bool Going { get; private set; }
        
        public DateTimeOffset AcceptedAt { get; private set; }

        public void Accept(DateTimeOffset acceptedAt)
        {
            AcceptedAt = acceptedAt;
            Going = true;
        }

        public void Decline() => Going = false;
    }
}