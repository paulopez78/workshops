using Meetup.Scheduling.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

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

            // var defaultCapacity = Configuration.GetValue<int>("MeetupEvents:DefaultCapacity");
            // var meetupOptions = new MeetupEventsOptions(100);

            // Configuration.GetSection("MeetupEvents").Bind(meetupOptions);
            // var meetupOptions = Configuration.GetSection("MeetupEvents").Get<MeetupEventsOptions>();

            services.Configure<MeetupEventsOptions>(Configuration.GetSection("MeetupEvents"));
            var connectionString = Configuration.GetConnectionString("MeetupEvents");
            
            // services.AddEntityFrameworkNpgsql();
            services.AddDbContext<MeetupDbContext>(options => options.UseNpgsql(connectionString));

            //services.AddSingleton(new InMemoryDatabase());
            //services.AddSingleton<MeetupEventRepository>();
            
            services.AddScoped<IRepository, MeetupEventPostgresRepository>();
            services.AddScoped<MeetupEventApplicationService>();
            services.AddScoped<MeetupEventPostgresQueries>();
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