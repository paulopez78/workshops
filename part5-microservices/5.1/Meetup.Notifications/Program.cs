using System;
using GreenPipes;
using MassTransit;
using Meetup.Notifications.Application;
using Meetup.Notifications.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Meetup.Notifications
{
    public class Program
    {
        const string ApplicationKey = "meetup_notifications";
        
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var connectionString = hostContext.Configuration.GetConnectionString("Notifications");
                    var mongoDb          = CreateMongoDb(connectionString);
                    MongoConventions.RegisterConventions();

                    services.AddSingleton(mongoDb);
                    services.AddSingleton<NotificationsApplicationService>();

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<NotificationsCommandHandler>();
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host("localhost", "/", h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                            // https://masstransit-project.com/usage/exceptions.html
                            cfg.UseMessageRetry(r =>
                            {
                                r.Interval(3, TimeSpan.FromMilliseconds(100));
                                r.Ignore<ArgumentException>();
                            });

                            cfg.ReceiveEndpoint($"{ApplicationKey}-notify",
                                e => { e.Consumer<NotificationsCommandHandler>(context); });
                        });
                    });
                    services.AddMassTransitHostedService();

                    static IMongoDatabase CreateMongoDb(string connectionString)
                    {
                        var client = new MongoClient(connectionString);
                        return client.GetDatabase("meetup-notifications");
                    }
                });
    }
}