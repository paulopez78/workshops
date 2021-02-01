using System;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Meetup.Scheduling.AsyncProjector;
using Meetup.Scheduling.Contracts;
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
            services.AddMarten(cfg =>
            {
                cfg.Connection(hostContext.Configuration.GetConnectionString("MeetupScheduling"));
                cfg.AutoCreateSchemaObjects   = AutoCreate.All;
                cfg.DatabaseSchemaName        = "scheduling";
                cfg.Events.DatabaseSchemaName = "scheduling";
                cfg.Events.AsyncProjections.Add(new MeetupEventProjection());

                cfg.Schema.For<ReadModels.V1.MeetupEvent>().Index(x => x.Group);
                cfg.Schema.For<ReadModels.V1.MeetupEvent>().UniqueIndex(x => x.AttendantListId);
            });
            services.AddHostedService<AsyncProjectionsBackgroundService>();
        });