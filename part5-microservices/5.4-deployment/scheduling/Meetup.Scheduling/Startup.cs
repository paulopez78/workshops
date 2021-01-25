using System;
using GreenPipes;
using Marten;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Meetup.Scheduling.MeetupDetails;
using Meetup.Scheduling.AttendantList;
using Meetup.Scheduling.Contracts;
using Meetup.Scheduling.Framework;
using OpenTelemetry.Trace;
using Prometheus;
using static Meetup.Scheduling.Program;
using static Meetup.Scheduling.MeetupDetails.MeetupDetailsEventProjection;
using static Meetup.Scheduling.AttendantList.AttendantListProjection;
using static Meetup.Scheduling.Contracts.ReadModels.V1;

namespace Meetup.Scheduling
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTelemetryTracing(b =>
                b.AddAspNetCoreInstrumentation()
                    .AddMassTransitInstrumentation()
                    .AddJaegerExporter(o =>
                    {
                        o.ServiceName = ApplicationKey;
                        o.AgentHost   = Configuration["JAEGER_HOST"] ?? "localhost";
                    })
            );

            services.AddControllers();
            services.AddSingleton<UtcNow>(() => DateTimeOffset.UtcNow);

            services.AddScoped(sp =>
                sp.AddApplicationService<MeetupDetailsAggregate, MeetupDetailsEventReadModel>(When)
                    .Handle(sp.GetRequiredService<UtcNow>())
            );

            services.AddScoped(sp =>
                sp.AddApplicationService<AttendantListAggregate, AttendantListReadModel>(When)
                    .Handle(sp.GetRequiredService<UtcNow>())
                    .MappingId(sp.GetRequiredService<IDocumentStore>())
            );

            services.AddMarten(cfg =>
            {
                cfg.Connection(Configuration.GetConnectionString("MeetupScheduling"));
                cfg.AutoCreateSchemaObjects   = AutoCreate.CreateOrUpdate;
                cfg.DatabaseSchemaName        = "scheduling";
                cfg.Events.DatabaseSchemaName = "scheduling";

                cfg.Schema.For<MeetupDetailsEventReadModel>().Index(x => x.Group);
                cfg.Schema.For<AttendantListReadModel>().UniqueIndex(x => x.MeetupEventId);
                cfg.Schema.For<OutBox>().Index(x => new {x.MessageId, x.AggregateId});
            });

            services.AddMassTransit(x =>
            {
                x.AddConsumer<AttendantListCommandApiAsync>();
                x.AddConsumer<AttendantListBatchCommandApiAsync>();
                x.AddConsumer<MeetupDetailsCommandApiAsync>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration["RabbitMQ:Host"], "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    // https://masstransit-project.com/usage/exceptions.html
                    cfg.UseMessageRetry(r =>
                    {
                        r.Interval(3, TimeSpan.FromMilliseconds(100));
                        r.Handle<ApplicationException>();
                        // r.Ignore<ArgumentException>();
                    });
                    var attendantListQueue = $"{ApplicationKey}-attendant-list-commands";
                    cfg.ReceiveEndpoint(attendantListQueue,
                        e =>
                        {
                            e.Consumer<AttendantListCommandApiAsync>(context);
                            e.Consumer<AttendantListBatchCommandApiAsync>(context);
                        });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-meetup-details-commands",
                        e => { e.Consumer<MeetupDetailsCommandApiAsync>(context); });

                    EndpointConvention.Map<AttendantListCommands.V1.DontAttend>(new Uri($"queue:{attendantListQueue}"));
                });
            });
            services.AddMassTransitHostedService();

            services.AddSwaggerGen(c =>
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "meetup_scheduling_commands", Version = "v1"})
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "meetup scheduling commands v1"));
            }

            app.UseRouting();
            app.UseHttpMetrics();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}