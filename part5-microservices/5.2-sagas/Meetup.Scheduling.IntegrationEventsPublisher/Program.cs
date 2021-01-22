using System;
using GreenPipes;
using MassTransit;
using Meetup.Scheduling.IntegrationEventsPublisher;
using Microsoft.Extensions.Hosting;

CreateHostBuilder(args).Build().Run();

IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            const string ApplicationKey = "meetup_scheduling";
            services.AddMassTransit(x =>
            {
                x.AddConsumer<IntegrationEventsPublisher>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromMilliseconds(100));
                    });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-publish-integration-event",
                        e => { e.Consumer<IntegrationEventsPublisher>(context); });
                });
            });

            services.AddMassTransitHostedService();
        });