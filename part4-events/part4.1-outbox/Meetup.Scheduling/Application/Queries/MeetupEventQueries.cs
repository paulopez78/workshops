using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Meetup.Scheduling.Application.Queries
{
    public class MeetupEventPostgresQueries
    {
        public MeetupEventPostgresQueries(IConfiguration configuration)
            => ConnectionString = configuration.GetConnectionString("MeetupEvents");

        private readonly string ConnectionString;

        const string BaseQuery =
            "SELECT M.\"Id\", M.\"Title\", M.\"Group\", AL.\"Capacity\", M.\"Status\", A.\"Id\", A.\"UserId\", A.\"Status\", A.\"ModifiedAt\" " +
            "FROM \"MeetupEvent\" M " +
            "LEFT JOIN \"AttendantList\" AL on M.\"Id\" = AL.\"Id\" " +
            "LEFT JOIN \"Attendant\" A on AL.\"Id\" = A.\"AttendantListId\"";

        public async Task<MeetupEvent?> Handle(V1.GetById query)
        {
            await using var dbConnection = new Npgsql.NpgsqlConnection(ConnectionString);

            MeetupEvent? result = null;

            await dbConnection.QueryAsync<MeetupEvent, Attendant, MeetupEvent>($"{BaseQuery} WHERE M.\"Id\"=@id",
                (evt, inv) =>
                {
                    result ??= evt;
                    if (inv is not null) result.Attendants.Add(inv);
                    return result;
                },
                new {Id = query.EventId});

            return result;
        }

        public async Task<IEnumerable<MeetupEvent>> Handle(V1.GetByGroup query)
        {
            await using var dbConnection = new Npgsql.NpgsqlConnection(ConnectionString);

            var lookup = new Dictionary<Guid, MeetupEvent>();

            await dbConnection.QueryAsync<MeetupEvent, Attendant, MeetupEvent>($"{BaseQuery} WHERE M.\"Group\"=@group",
                (evt, inv) =>
                {
                    if (!lookup.ContainsKey(evt.Id)) lookup.Add(evt.Id, evt);

                    var meetupEvent = lookup[evt.Id];
                    if (inv is not null) meetupEvent.Attendants.Add(inv);
                    return meetupEvent;
                },
                new {query.Group});

            return lookup.Values;
        }
    }
}