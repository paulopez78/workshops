using MeetupEvents.Infrastructure;
using MeetupEvents.Queries;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Xunit.Abstractions;

namespace MeetupEvents.Test
{
    public class WebApiFixture : WebApplicationFactory<Startup>
    {
        public ITestOutputHelper Output { get; set; }

        public MeetupEventQueries Queries { get; set; }

        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = base.CreateHostBuilder();

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddXUnit(Output);
            });

            return builder;
        }

        public MeetupEvent.MeetupEventClient CreateGrpcClient()
        {
            var client = CreateDefaultClient();
            var channel = GrpcChannel.ForAddress(client.BaseAddress, new GrpcChannelOptions
            {
                HttpClient = client
            });

            return new MeetupEvent.MeetupEventClient(channel);
        }

        // migrate automatically database
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var connectionString = services.BuildServiceProvider().GetService<IConfiguration>()
                    .GetConnectionString("MeetupEvents");

                Queries = new MeetupEventQueries(
                    () => new NpgsqlConnection(connectionString)
                );

                using var scope = services.BuildServiceProvider().CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<MeetupDbContext>();
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
            });
        }
    }
}