using System;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using static MeetupEvents.EventsDispatcher.DomainServices;

namespace MeetupEvents.EventsDispatcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var connectionString = hostContext.Configuration.GetConnectionString("MeetupEvents");

                    services.AddSingleton<GetMeetupEventId>(id =>
                        GetMeetupEventId(() => new NpgsqlConnection(connectionString), id)
                    );

                    services.AddSingleton<GetMeetupDetails>(id =>
                        GetMeetupDetails(() => new NpgsqlConnection(connectionString), id)
                    );

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<IntegrationEventsDispatcher>();
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(hostContext.Configuration.GetValue("RabbitMQ:Host", "localhost"), "/", h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                            cfg.UseMessageRetry(r => { r.Interval(5, TimeSpan.FromMilliseconds(100)); });

                            cfg.ReceiveEndpoint("publish-integration-events",
                                e => e.Consumer<IntegrationEventsDispatcher>(context));
                        });
                    });
                    services.AddMassTransitHostedService();
                });
    }
}