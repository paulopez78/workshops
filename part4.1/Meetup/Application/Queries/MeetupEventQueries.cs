using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Meetup.Scheduling.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Meetup.Scheduling.Application.Queries
{
    public class MeetupEventQueries
    {
        readonly InMemoryDatabase Database;

        public MeetupEventQueries(InMemoryDatabase inMemoryDatabase) => Database = inMemoryDatabase;

        public Domain.MeetupEvent? Get(Guid id)
            => Database.MeetupEvents.FirstOrDefault(x => x.Id == id);

        public IEnumerable<Domain.MeetupEvent> GetByGroup(string group)
            => Database.MeetupEvents.Where(x => x.Group == group);
    }

    public class MeetupEventPostgresQueries
    {
        public MeetupEventPostgresQueries(IConfiguration configuration)
            => ConnectionString = configuration.GetConnectionString("MeetupEvents");

        private readonly string ConnectionString;

        public async Task<MeetupEvent?> Get(Guid id)
        {
            await using var dbConnection = new Npgsql.NpgsqlConnection(ConnectionString);

            MeetupEvent? result = null;

            await dbConnection.QueryAsync<MeetupEvent, Attendant, MeetupEvent>(
                "SELECT M.\"Id\", M.\"Title\", M.\"Group\", M.\"Capacity\", M.\"Status\", A.\"Id\", A.\"UserId\", A.\"Status\" " +
                "FROM \"MeetupEvents\" M LEFT JOIN \"Attendant\" A ON M.\"Id\" = A.\"MeetupEventId\" " +
                "WHERE M.\"Id\"=@id",
                (evt, inv) =>
                {
                    result ??= evt;
                    if (inv is not null) result.Attendants.Add(inv);
                    return result;
                },
                new {Id = id});

            return result;
        }

        public async Task<IEnumerable<MeetupEvent>> GetByGroup(string group)
        {
            await using var dbConnection = new Npgsql.NpgsqlConnection(ConnectionString);

            var lookup = new Dictionary<Guid, MeetupEvent>();

            await dbConnection.QueryAsync<MeetupEvent, Attendant, MeetupEvent>(
                "SELECT M.\"Id\", M.\"Title\", M.\"Group\", M.\"Capacity\", M.\"Status\", A.\"Id\", A.\"UserId\", A.\"Status\" " +
                "FROM \"MeetupEvents\" M LEFT JOIN \"Attendant\" A ON M.\"Id\" = A.\"MeetupEventId\" " +
                "WHERE M.\"Group\"=@group",
                (evt, inv) =>
                {
                    if (!lookup.ContainsKey(evt.Id)) lookup.Add(evt.Id, evt);

                    var meetupEvent = lookup[evt.Id];
                    if (inv is not null) meetupEvent.Attendants.Add(inv);
                    return meetupEvent;
                },
                new {Group = group});

            return lookup.Values;
        }
    }

    # nullable disable
    public record MeetupEvent
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Group { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
        public List<Attendant> Attendants{ get; set; } = new();
    }

    public record Attendant(Guid Id, Guid UserId, string Status);
}