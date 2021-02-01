using System;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;

namespace MeetupEvents.Application
{
    public delegate Task<Guid?>? GetMeetupEventId(Guid id);

    public static class DomainServices
    {
        public static async Task<Guid?> GetAttendantListId(Func<DbConnection> getDbConnection, Guid meetupId)
        {
            await using var connection = getDbConnection();

            return await connection.QuerySingleOrDefaultAsync<Guid>(
                "SELECT AL.\"Id\" FROM \"AttendantList\" AL WHERE AL.\"MeetupEventId\" = @id",
                new {Id = meetupId}
            );
        }

        public static async Task<Guid?> GetMeetupEventId(Func<DbConnection> getDbConnection, Guid attendantListId)
        {
            await using var connection = getDbConnection();

            return await connection.QuerySingleOrDefaultAsync<Guid>(
                "SELECT AL.\"MeetupEventId\" FROM \"AttendantList\" AL WHERE AL.\"Id\" = @id",
                new {Id = attendantListId}
            );
        }
    }
}