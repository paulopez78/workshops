using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Meetup.Scheduling.AsyncProjector;
using Meetup.Scheduling.Contracts;

CreateHostBuilder(args).Build().Run();

IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
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