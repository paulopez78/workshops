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
using Meetup.Scheduling.Queries;

namespace Meetup.Scheduling
{
    public class Startup
    {
        const string ApplicationKey = "meetup_scheduling";

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var connectionString = Configuration.GetConnectionString("MeetupScheduling");
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


            services.AddMassTransit(x =>
            {
                x.AddConsumer<MeetupCreatedDomainEventHandler>();
                x.AddConsumer<MeetupCanceledDomainEventHandler>();
                x.AddConsumer<MeetupPublishedDomainEventHandler>();
                x.AddConsumer<GroupMemberLeftEventHandler>();
                x.AddConsumer<IntegrationEventsPublisher>();

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

                    cfg.ReceiveEndpoint($"{ApplicationKey}-create-attendant-list",
                        e => { e.Consumer<MeetupCreatedDomainEventHandler>(context); });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-open-attendant-list",
                        e => { e.Consumer<MeetupPublishedDomainEventHandler>(context); });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-close-attendant-list",
                        e => { e.Consumer<MeetupCanceledDomainEventHandler>(context); });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-remove-member-from-attendant-lists",
                        e => { e.Consumer<GroupMemberLeftEventHandler>(context); });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-publish-integration-event",
                        e => { e.Consumer<IntegrationEventsPublisher>(context); });
                });
            });
            services.AddMassTransitHostedService();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "meetupevents", Version = "v1"});
            });

            void AddEventStore()
            {
                services.AddMarten(cfg =>
                {
                    cfg.Connection(connectionString);
                    cfg.AutoCreateSchemaObjects   = AutoCreate.All;
                    cfg.DatabaseSchemaName        = "scheduling";
                    cfg.Events.DatabaseSchemaName = "scheduling";
                    cfg.Events.AsyncProjections.Add(new MeetupEventProjection());

                    cfg.Schema.For<MeetupDetailsEventProjection.MeetupDetailsEvent>()
                        .Index(x => x.Group, x => x.IndexName = "mt_doc_meetupdetailsevent_idx_group");

                    cfg.Schema.For<AttendantListProjection.AttendantList>()
                        .UniqueIndex("mt_doc_attendantlist_uidx_meetup_event_id", x => x.MeetupEventId);

                    cfg.Schema.For<MeetupEvent>().Index(x => x.Group);
                    cfg.Schema.For<MeetupEvent>().UniqueIndex(x => x.AttendantListId);

                    cfg.Schema.For<OutBox>()
                        .Index(x => new {x.MessageId, x.AggregateId});
                });
                services.AddHostedService<AsyncProjectionsBackgroundService>();
            }
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