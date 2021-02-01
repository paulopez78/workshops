using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace MeetupEvents.Queries
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueries(this IServiceCollection services, string connectionString) =>
            services.AddSingleton(
                new MeetupEventQueries(
                    () => new NpgsqlConnection(connectionString)
                )
            );
    }
}