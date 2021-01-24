using System;
using GreenPipes;
using MassTransit;
using Meetup.Notifications.Contracts;
using Meetup.Scheduling.Contracts;
using Meetup.Scheduling.ProcessManager;
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

IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<MeetupProcessManager>();
                x.AddRabbitMqMessageScheduler();
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UseDelayedExchangeMessageScheduler();
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    // https://masstransit-project.com/usage/exceptions.html
                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromMilliseconds(100));
                        r.Handle<ApplicationException>();
                        r.Ignore<ArgumentException>();
                    });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-meetup-saga",
                        e => { e.Consumer<MeetupProcessManager>(context); });

                    var attendantListQueue = new Uri($"queue:{ApplicationKey}-attendant-list-commands");
                    var meetupDetailsQueue = new Uri($"queue:{ApplicationKey}-meetup-details-commands");
                    var notificationsQueue = new Uri("queue:meetup_notifications-commands");
                    EndpointConvention.Map<AttendantListCommands.V1.CreateAttendantList>(attendantListQueue);
                    EndpointConvention.Map<AttendantListCommands.V1.Open>(attendantListQueue);
                    EndpointConvention.Map<AttendantListCommands.V1.Close>(attendantListQueue);
                    EndpointConvention.Map<AttendantListCommands.V1.Archive>(attendantListQueue);
                    EndpointConvention.Map<AttendantListCommands.V1.RemoveAttendantFromMeetups>(attendantListQueue);
                    EndpointConvention.Map<MeetupDetailsCommands.V1.Start>(meetupDetailsQueue);
                    EndpointConvention.Map<MeetupDetailsCommands.V1.Finish>(meetupDetailsQueue);
                    EndpointConvention.Map<Commands.V1.NotifyMeetupPublished>(notificationsQueue);
                    EndpointConvention.Map<Commands.V1.NotifyMeetupCancelled>(notificationsQueue);
                    EndpointConvention.Map<Commands.V1.NotifyMeetupAttendantGoing>(notificationsQueue);
                    EndpointConvention.Map<Commands.V1.NotifyMeetupAttendantWaiting>(notificationsQueue);
                });
            });

            services.AddMassTransitHostedService();
        });