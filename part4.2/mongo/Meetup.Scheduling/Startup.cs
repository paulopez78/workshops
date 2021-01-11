using System;
using Meetup.Scheduling.Application.AttendantList;
using Meetup.Scheduling.Application.Details;
using Meetup.Scheduling.Application.Queries;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

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
            var mongoDb          = CreateMongoDb(connectionString);            
            MongoConventions.RegisterConventions();
            
            services.AddSingleton(mongoDb);

            services.AddSingleton<IDateTimeProvider>(new DateTimeProvider());
            services.AddSingleton<UtcNow>(() => DateTimeOffset.UtcNow);

            services.AddSingleton<MongoDbRepository<MeetupEventDetailsAggregate>>();
            services.AddScoped<MeetupEventDetailsApplicationService>();

            services.AddSingleton<MongoDbRepository<AttendantListAggregate>>();
            services.AddScoped<AttendantListApplicationService>();

            services.AddScoped<MeetupEventMongoQueries>();
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


        static IMongoDatabase CreateMongoDb(string connectionString)
        {
            var client = new MongoClient(connectionString);
            return client.GetDatabase("meetup");
        }
    }
}