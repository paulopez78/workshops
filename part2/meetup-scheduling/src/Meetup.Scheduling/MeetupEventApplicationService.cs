using System;
using System.Collections.Generic;
using System.Linq;

namespace Meetup.Scheduling
{
    public class MeetupApplicationService
    {
        Dictionary<Guid, MeetupEventEntity> MeetupEvents { get; }

        public MeetupApplicationService() => MeetupEvents = new();

        public Guid Add(string group, MeetupEvent meetupEvent)
        {
            var eventId = Guid.NewGuid();

            MeetupEvents.Add(
                eventId,
                new MeetupEventEntity(
                    group,
                    meetupEvent.Title,
                    meetupEvent.Capacity,
                    meetupEvent.Published,
                    new()));

            return eventId;
        }

        public void Publish(Guid eventId)
        {
            var value = Get(eventId);
            if (value is not null) MeetupEvents[eventId] = value with { Published = true };
        }

        public void Remove(Guid eventId)
            => MeetupEvents.Remove(eventId);

        public MeetupEventEntity? Get(Guid eventId)
            => MeetupEvents.TryGetValue(eventId, out var events) ? events : null;

        public List<MeetupEventEntity> GetAll(string group) =>
            MeetupEvents
                .Where(x => x.Value.Group == group)
                .Select(x => x.Value)
                .ToList();

        public bool AcceptInvitation(AcceptInvitation command) =>
            UpdateInvitationResponse(command.EventId, command.UserId, true);

        public bool DeclineInvitation(DeclineInvitation command) =>
            UpdateInvitationResponse(command.EventId, command.UserId, false);

        bool UpdateInvitationResponse(Guid eventId, Guid userId, bool going)
        {
            var meetupEvent = Get(eventId);

            if (meetupEvent is not null && HasCapacity(meetupEvent))
            {
                meetupEvent.InvitationResponse.RemoveAll(x => x.UserId == userId);
                meetupEvent.InvitationResponse.Add(new InvitationResponse(userId, going));
                return true;
            }

            return false;
        }

        static bool HasCapacity(MeetupEventEntity meetupEvent)
            => meetupEvent.Capacity > meetupEvent.InvitationResponse.Count(x => x.Going);


        public void IncreaseCapacity(IncreaseCapacity command)
        {
            var meetupEvent = Get(command.EventId);
            if (meetupEvent is not null)
            {
                var newCapacity = meetupEvent.Capacity + command.Capacity;
                MeetupEvents[command.EventId] = meetupEvent with { Capacity = newCapacity};
            }
        }

        public void ReduceCapacity(ReduceCapacity command)
        {
            var meetupEvent = Get(command.EventId);
            if (meetupEvent is not null)
            {
                var newCapacity = meetupEvent.Capacity - command.Capacity;
                MeetupEvents[command.EventId] = meetupEvent with { Capacity = newCapacity };
            }
        }
    }

    public record AcceptInvitation(Guid EventId, Guid UserId);

    public record DeclineInvitation(Guid EventId, Guid UserId);

    public record IncreaseCapacity(Guid EventId, int Capacity);


    public record ReduceCapacity(Guid EventId, int Capacity);

    public record MeetupEventEntity(string Group, string Title, int Capacity, bool Published,
        List<InvitationResponse> InvitationResponse);

    public record InvitationResponse(Guid UserId, bool Going);
}