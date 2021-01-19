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
using Meetup.Scheduling.Framework;

namespace Meetup.Scheduling
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var connectionString = Configuration.GetConnectionString("MeetupEvents");
            AddEventStore();

            services.AddSingleton<UtcNow>(() => DateTimeOffset.UtcNow);

            services.AddScoped(sp =>
                sp.AddApplicationService<MeetupEventDetailsAggregate, MeetupDetailsEventProjection.MeetupDetailsEvent>(
                    MeetupDetailsEventProjection.When
                ).Handle(sp.GetRequiredService<UtcNow>()));

            services.AddScoped(sp =>
                sp.AddApplicationService<AttendantListAggregate, AttendantListProjection.AttendantList>(
                    AttendantListProjection.When
                ).Handle(sp.GetRequiredService<UtcNow>()));

            services.AddScoped<MeetupCreatedMassTransitDomainEventHandler>();
            services.AddScoped<MeetupCanceledMassTransitDomainEventHandler>();
            services.AddScoped<MeetupPublishedMassTransitDomainEventHandler>();
            services.AddScoped<Notifications.MeetupPublishedMassTransitDomainEventHandler>();

            services.AddMassTransit(x =>
            {
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
                        r.Handle<ApplicationException>();
                        r.Ignore<ArgumentException>();
                    });

                    cfg.ReceiveEndpoint("create-attendant-list",
                        e => { e.Consumer<MeetupCreatedMassTransitDomainEventHandler>(context); });

                    cfg.ReceiveEndpoint("open-attendant-list",
                        e => { e.Consumer<MeetupPublishedMassTransitDomainEventHandler>(context); });

                    cfg.ReceiveEndpoint("close-attendant-list",
                        e => { e.Consumer<MeetupCanceledMassTransitDomainEventHandler>(context); });

                    cfg.ReceiveEndpoint("send-notification",
                        e => { e.Consumer<Notifications.MeetupPublishedMassTransitDomainEventHandler>(context); });
                });
            });
            services.AddMassTransitHostedService();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "meetupevents", Version = "v1"});
            });

            void AddEventStore() =>
                services.AddMarten(cfg =>
                {
                    cfg.Connection(connectionString);
                    cfg.AutoCreateSchemaObjects   = AutoCreate.All;
                    cfg.DatabaseSchemaName        = "scheduling";
                    cfg.Events.DatabaseSchemaName = "scheduling";

                    cfg.Schema.For<MeetupDetailsEventProjection.MeetupDetailsEvent>()
                        .Index(x => x.Group, x => x.IndexName = "mt_doc_meetupdetailsevent_idx_group");

                    cfg.Schema.For<AttendantListProjection.AttendantList>()
                        .UniqueIndex("mt_doc_attendantlist_uidx_meetup_event_id", x => x.MeetupEventId);

                    cfg.Schema.For<OutBox>()
                        .Index(x => new {x.MessageId, x.AggregateId});
                });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "meetupevents v1"));
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}