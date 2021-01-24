using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using static Meetup.Scheduling.Contracts.ReadModels.V1;

namespace Meetup.Scheduling.Queries
{
    public static class MeetupSchedulingQueries
    {
        public static async Task<MeetupEvent?> HandleWithAsyncProjection(this IDocumentStore store, V1.GetById query)
        {
            using var session = store.QuerySession();

            var result = await session.LoadAsync<MeetupEvent>(query.EventId);
            return result;
        }

        public static async Task<IEnumerable<MeetupEvent>> HandleWithAsyncProjection(this IDocumentStore store,
            V1.GetByGroup query)
        {
            using var session = store.QuerySession();
            var       result  = await session.Query<MeetupEvent>().Where(x => x.Group == query.Group).ToListAsync();
            return result;
        }

        public static async Task<MeetupEvent?> Handle(this IDocumentStore store, V1.GetById query)
        {
            using var session = store.QuerySession();

            var meetupEvent = await session.LoadAsync<MeetupDetailsEventReadModel>(query.EventId);
            var attendantList = await
                session.Query<AttendantListReadModel>()
                    .FirstOrDefaultAsync(x => x.MeetupEventId == query.EventId);

            return Map(meetupEvent, attendantList);
        }

        public static async Task<IEnumerable<MeetupEvent>> Handle(this IDocumentStore store, V1.GetByGroup query)
        {
            using var session = store.QuerySession();

            var meetups = await session.Query<MeetupDetailsEventReadModel>()
                .Where(x => x.Group == query.Group).ToListAsync();
            var meetupIds = meetups.Select(x => x.Id).ToArray();

            var attendantLists = await session.Query<AttendantListReadModel>()
                .Where(x => x.MeetupEventId.In(meetupIds)).ToListAsync();

            return meetups.Select(
                x => Map(x, attendantLists.FirstOrDefault(y => y.Id == y.MeetupEventId))
            );
        }

        static MeetupEvent Map(
            MeetupDetailsEventReadModel details,
            AttendantListReadModel? attendants) =>
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

        static Attendant Map(Attendant attendant) =>
            new()
            {
                UserId = attendant.UserId, Waiting = attendant.Waiting, AddedAt = attendant.AddedAt
            };
    }
}