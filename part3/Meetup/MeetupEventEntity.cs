using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.Scheduling
{
    public class MeetupEventEntity
    {
        public readonly Guid Id;

        public readonly string Group;
        public string Title { get; }
        public int Capacity { get; private set; }

        public MeetupEventStatus Status { get; private set; } = MeetupEventStatus.Draft;

        public IReadOnlyList<InvitationResponse> Invitations => _invitations;

        readonly List<InvitationResponse> _invitations = new();

        public MeetupEventEntity(Guid id, string group, string title, int capacity)
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
            => Capacity = Capacity + increase;

        public void ReduceCapacity(int decrease)
        {
            var newCapacity = Capacity - decrease;
            Capacity = newCapacity < 1 ? 0 : newCapacity;
        }

        public void AcceptInvitation(Guid userId)
        {
            if (Status is not MeetupEventStatus.Scheduled)
                throw new ApplicationException("Meetup not scheduled");

            if (NotEnoughCapacity())
                throw new ApplicationException("Meetup event doesnt have enough capacity");

            UpdateInvitation(userId, true);
        }


        public void DeclineInvitation(Guid userId)
        {
            if (Status is not MeetupEventStatus.Scheduled)
                throw new ApplicationException("Meetup not scheduled");
            
            UpdateInvitation(userId, false);
        }
        
        void UpdateInvitation(Guid userId, bool going)
        {
            _invitations.RemoveAll(x => x.UserId == userId);
            _invitations.Add(new InvitationResponse(userId, going));
        }

        bool NotEnoughCapacity() => Invitations.Count(x => x.Going) >= Capacity;
    }

    public enum MeetupEventStatus
    {
        Draft,
        Scheduled,
        Cancelled
    }

    public class InvitationResponse
    {
        public InvitationResponse(Guid userId, bool going)
        {
            UserId = userId;
            Going = going;
        }
        
        public bool Going { get; }

        public Guid UserId { get; }
    }
}