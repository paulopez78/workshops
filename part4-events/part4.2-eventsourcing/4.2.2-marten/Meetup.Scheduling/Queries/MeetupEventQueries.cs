using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using Meetup.Scheduling.AttendantList;
using Meetup.Scheduling.MeetupDetails;

namespace Meetup.Scheduling.Queries
{
    public static class MeetupSchedulingQueries
    {
        public static async Task<MeetupEvent?> Handle(this IDocumentStore store, V1.GetById query)
        {
            using var session = store.QuerySession();

            var meetupEvent = await session.LoadAsync<MeetupDetailsEventProjection.MeetupDetailsEvent>(query.EventId);
            var attendantList = await
                session.Query<AttendantListProjection.AttendantList>()
                    .FirstOrDefaultAsync(x => x.MeetupEventId == query.EventId);

            return Map(meetupEvent, attendantList);
        }

        public static async Task<MeetupEvent?> HandleWithProjection(this IDocumentStore store, V1.GetById query)
        {
            using var session = store.QuerySession();

            var result = await session.LoadAsync<MeetupEvent>(query.EventId);
            return result;
        }

        public static async Task<IEnumerable<MeetupEvent>> Handle(this IDocumentStore store, V1.GetByGroup query)
        {
            using var session = store.QuerySession();

            var meetups = await session.Query<MeetupDetailsEventProjection.MeetupDetailsEvent>()
                .Where(x => x.Group == query.Group).ToListAsync();
            var meetupIds = meetups.Select(x => x.Id).ToArray();

            var attendantLists = await session.Query<AttendantListProjection.AttendantList>()
                .Where(x => x.MeetupEventId.In(meetupIds)).ToListAsync();

            return meetups.Select(
                x => Map(x, attendantLists.FirstOrDefault(y => y.Id == y.MeetupEventId))
            );
        }

        public static async Task<Guid?> GetAttendantListId(this IDocumentStore store, Guid meetupId)
        {
            using var session = store.QuerySession();
            var result = await session.Query<AttendantListProjection.AttendantList>()
                .FirstOrDefaultAsync(x => x.MeetupEventId == meetupId);
            return result.Id;
        }

        static MeetupEvent Map(
            MeetupDetailsEventProjection.MeetupDetailsEvent details,
            AttendantListProjection.AttendantList? attendants) =>
            new()
            {
                Id              = details.Id,
                Title           = details.Title,
                Description     = details.Description,
                Group           = details.Group,
                Capacity        = details.Capacity,
                Status          = details.Status,
                Start           = details.Start,
                End             = details.End,
                Location        = details.Location,
                Online          = details.Online,
                AttendantListId = attendants?.Id,
                Attendants      = attendants?.Attendants.Select(Map).ToImmutableList()
            };

        static Attendant Map(AttendantListProjection.AttendantList.Attendant attendant) =>
            new()
            {
                UserId = attendant.UserId, Waiting = attendant.Waiting, AddedAt = attendant.AddedAt
            };
    }
}