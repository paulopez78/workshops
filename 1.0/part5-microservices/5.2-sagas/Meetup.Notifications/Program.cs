using System;
using System.Net.Http;
using GreenPipes;
using Grpc.Net.ClientFactory;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Polly;
using Meetup.Notifications.Application;
using Meetup.Notifications.Infrastructure;
using Meetup.GroupManagement.Contracts.Queries.V1;
using static Meetup.Notifications.Infrastructure.HttpPolicies;
using static Meetup.Notifications.Application.ExternalServices;

CreateHostBuilder(args).Build().Run();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            const string ApplicationKey = "meetup_notifications";

            MongoConventions.RegisterConventions();
            var mongoDb = CreateMongoDb();
            services.AddSingleton(mongoDb);

            // add external services
            AddMeetupGroupGrpcClient();
            AddMeetupSchedulingHttpClient();
            services.AddSingleton(sp => GetGroupMembers(() => GetGrpcClient(sp)));
            services.AddSingleton(sp => GetGroupOrganizer(() => GetGrpcClient(sp)));
            services.AddSingleton(sp => GetMeetupAttendants(() => GetHttpClient(sp)));
            services.AddSingleton(GetInterestedUsers());

            services.AddSingleton<NotificationsApplicationService>();
            services.AddMassTransit(x =>
            {
                x.AddConsumer<NotificationsCommandHandler>();
                x.AddConsumer<NotificationsEventHandler>();
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
                        r.Ignore<ArgumentException>();
                    });

                    cfg.ReceiveEndpoint($"{ApplicationKey}-commands",
                        e =>
                        {
                            e.Consumer<NotificationsCommandHandler>(context);
                            e.Consumer<NotificationsEventHandler>(context);
                        });
                });
            });
            services.AddMassTransitHostedService();

            IMongoDatabase CreateMongoDb()
            {
                var client = new MongoClient(hostContext.Configuration.GetConnectionString("Notifications"));
                return client.GetDatabase(ApplicationKey);
            }

            IHttpClientBuilder AddMeetupGroupGrpcClient()
            {
                var address = hostContext.Configuration["GroupManagement:Address"];
                return services.AddGrpcClient<MeetupGroupQueries.MeetupGroupQueriesClient>(
                        "MeetupGroupQueries",
                        o => o.Address = new Uri(address))
                    .AddPolicyHandler(RetryPolicy())
                    .AddPolicyHandler(GetCircuitBreakerPolicy());
            }

            IHttpClientBuilder AddMeetupSchedulingHttpClient()
            {
                var address  = hostContext.Configuration["MeetupScheduling:Address"];
                var jitterer = new Random();
                return services.AddHttpClient("MeetupScheduling", c => c.BaseAddress = new Uri(address))
                    .AddTransientHttpErrorPolicy(p =>
                        p.WaitAndRetryAsync(3, // exponential back-off plus some jitter
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                            + TimeSpan.FromMilliseconds(jitterer.Next(0, 100))))
                    .AddPolicyHandler(GetCircuitBreakerPolicy());
            }

            MeetupGroupQueries.MeetupGroupQueriesClient GetGrpcClient(IServiceProvider sp)
                => sp.GetRequiredService<GrpcClientFactory>()
                    .CreateClient<MeetupGroupQueries.MeetupGroupQueriesClient>("MeetupGroupQueries");

            HttpClient GetHttpClient(IServiceProvider sp)
                => sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient("MeetupScheduling");
        });