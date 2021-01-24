using System;
using GreenPipes;
using MassTransit;
using Meetup.Scheduling.IntegrationEventsPublisher;
using Microsoft.Extensions.Hosting;
using Serilog;
using static System.Environment;

const string ApplicationKey = "meetup_scheduling";

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
    .Enrich.WithProperty(nameof(ApplicationKey), ApplicationKey)
    .CreateLogger();
try
{
    Log.Information("Starting up");
    await CreateHostBuilder(args).Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

CreateHostBuilder(args).Build().Run();

IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
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

                    cfg.UseMessageRetry(r => { r.Interval(3, TimeSpan.FromMilliseconds(100)); });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-publish-integration-event",
                        e => { e.Consumer<IntegrationEventsPublisher>(context); });
                });
            });

            services.AddMassTransitHostedService();
        });