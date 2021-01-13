using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meetup.Scheduling.Application.Queries.Data;
using Meetup.Scheduling.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Meetup.Scheduling.Application.Queries
{
    public class MeetupEventMongoQueries
    {
        readonly IMongoCollection<MeetupEventDocument>   MeetupEventCollection;
        readonly IMongoCollection<AttendantListDocument> AttendantsCollection;

        public MeetupEventMongoQueries(IMongoDatabase database)
        {
            MeetupEventCollection = database.GetCollection<MeetupEventDocument>("MeetupEventDetailsAggregate");
            AttendantsCollection  = database.GetCollection<AttendantListDocument>("AttendantListAggregate");
        }

        public async Task<MeetupEvent?> Handle(V1.GetById query)
        {
            //https: //www.axonize.com/blog/iot-technology/joining-collections-in-mongodb-using-the-c-driver-and-linq/
            var result =
                from m in MeetupEventCollection.AsQueryable().Where(x => x.Id == query.EventId)
                join a in AttendantsCollection.AsQueryable() on m.Id equals a.Id into attendantsList
                select new {Meetup = m, List = attendantsList};


            var queryResult = await result.SingleOrDefaultAsync();

            return Map(queryResult.Meetup, queryResult.List.SingleOrDefault()?.AttendantList);
        }

        public async Task<IEnumerable<MeetupEvent>> Handle(V1.GetByGroup query)
        {
            var result =
                from m in MeetupEventCollection.AsQueryable().Where(x => x.Group.Value == query.Group)
                join a in AttendantsCollection.AsQueryable() on m.Id equals a.Id into attendantsList
                select new {Meetup = m, List = attendantsList};

            var queryResult = await result.ToListAsync();
            return queryResult.Select(x => Map(x.Meetup, x.List?.SingleOrDefault()?.AttendantList));
        }

        static MeetupEvent Map(MeetupEventDocument meetup, Data.AttendantList? list) =>
            new(meetup.Id, meetup.Details.Title, meetup.Group.Value, list?.Capacity.Value ?? 0, meetup.Status)
            {
                Attendants = list?.Attendants.Select(Map).ToList() ?? new()
            };

        static Attendant Map(Data.Attendant attendant)
            => new(attendant.UserId, attendant.Status, attendant.ModifiedAt);
    }
}