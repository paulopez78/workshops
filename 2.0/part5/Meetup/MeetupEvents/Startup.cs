using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Npgsql;
using MeetupEvents.Application;
using MeetupEvents.Domain;
using MeetupEvents.Infrastructure;
using MeetupEvents.Queries;
using static MeetupEvents.Application.DomainServices;

namespace MeetupEvents
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("MeetupEvents");
            services.AddDbContext<MeetupDbContext>(o => o.UseNpgsql(connectionString));

            services.AddScoped<Repository<MeetupEventAggregate>>();
            services.AddScoped<AttendantListRepository>();

            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.Configure<MeetupEventOptions>(Configuration.GetSection("MeetupEventOptions"));

            services.AddScoped<MeetupEventsApplicationService>();
            services.AddScoped<AttendantListApplicationService>();

            services.AddSingleton<GetMapId>(id =>
                GetMapId(() => new NpgsqlConnection(connectionString), id)
            );

            services.AddDomainEventsDispatcher(typeof(AttendantListEventsHandler));

            services.AddSingleton(
                new MeetupEventQueries(
                    () => new NpgsqlConnection(connectionString)
                )
            );

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "MeetupEvents", Version = "v1"});
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeetupEvents v1"));
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}