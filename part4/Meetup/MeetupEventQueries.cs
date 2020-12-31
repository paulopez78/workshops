using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Meetup.Scheduling.Data
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

            var result = await dbConnection.QuerySingleOrDefaultAsync<MeetupEvent>(
                "SELECT M.\"Id\", M.\"Title\", M.\"Group\", M.\"Capacity\", M.\"Status\", I.\"Id\", I.\"UserId\", I.\"Going\" "+
                "FROM \"MeetupEvents\" M INNER JOIN \"Invitation\" I ON M.\"Id\" = I.\"MeetupEventId\" " +
                "WHERE M.\"Id\"=@id",
                new {Id = id});

            return result;
        }

        public async Task<IEnumerable<MeetupEvent>> GetByGroup(string group)
        {
            await using var dbConnection = new Npgsql.NpgsqlConnection(ConnectionString);

            var lookup = new Dictionary<Guid, MeetupEvent>();

            await dbConnection.QueryAsync<MeetupEvent, Invitation, MeetupEvent>(
                "SELECT M.\"Id\", M.\"Title\", M.\"Group\", M.\"Capacity\", M.\"Status\", I.\"Id\", I.\"UserId\", I.\"Going\" " +
                "FROM \"MeetupEvents\" M INNER JOIN \"Invitation\" I ON M.\"Id\" = I.\"MeetupEventId\" " +
                "WHERE M.\"Group\"=@group",
                (evt, inv) =>
                {
                    if (lookup.TryGetValue(evt.Id, out var meetupEvent))
                    {
                        meetupEvent.Invitations.Add(inv);
                    }
                    else
                    {
                        evt.Invitations.Add(inv);
                        lookup.Add(evt.Id, evt);
                    }
                
                    return evt;
                },
                new {Group = group});

            return lookup.Values;
        }
    }

    public record MeetupEvent
    {
        public Guid Id { get; }
        public string Title { get; }
        public string Group { get; }
        public int Capacity { get; }
        public int Status { get; }
        public List<Invitation> Invitations { get; } = new();
    }

    public record Invitation(Guid Id, Guid UserId, bool Going);
}