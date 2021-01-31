using System;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;

namespace MeetupEvents.Application
{
    public static class DomainServices
    {
        public static async Task<Guid?> GetMapId(Func<DbConnection> getDbConnection, Guid id)
        {
            await using var connection = getDbConnection();

            return await connection.QuerySingleOrDefaultAsync<Guid>(
                "SELECT AL.\"Id\" FROM \"AttendantList\" AL WHERE AL.\"MeetupEventId\" = @id",
                new {id}
            );
        }
    }
}