using System;
using GreenPipes;
using MassTransit;
using Meetup.Scheduling.Application.AttendantList;
using Meetup.Scheduling.Application.Details;
using Meetup.Scheduling.Application.Queries;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Npgsql;

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
            services.AddDbContext<MeetupSchedulingDbContext>(options =>
                options.UseNpgsql(connectionString).EnableSensitiveDataLogging());

            services.AddSingleton<IDateTimeProvider>(new DateTimeProvider());
            services.AddSingleton<UtcNow>(() => DateTimeOffset.UtcNow);

            services.AddScoped<MeetupRepository<MeetupEventDetailsAggregate>>();
            services.AddScoped<MeetupEventDetailsApplicationService>();

            services.AddScoped<MeetupRepository<AttendantListAggregate>>();
            services.AddScoped<AttendantListApplicationService>();

            services.AddScoped<MeetupEventPostgresQueries>();

            // services.AddDomainEventsDispatcher(typeof(MeetupCreatedDomainEventHandler));
            // services.AddHostedService<OutboxProcessor>();

            services.AddScoped<MeetupCreatedMassTransitDomainEventHandler>();
            services.AddScoped<MeetupCanceledMassTransitDomainEventHandler>();
            services.AddScoped<MeetupPublishedMassTransitDomainEventHandler>();
            services.AddScoped<Application.Notifications.MeetupPublishedMassTransitDomainEventHandler>();

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
                        r.Handle<NpgsqlException>();
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
                        e =>
                        {
                            e.Consumer<Application.Notifications.MeetupPublishedMassTransitDomainEventHandler>(context);
                        });
                });
            });
            services.AddMassTransitHostedService();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "meetupevents", Version = "v1"});
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