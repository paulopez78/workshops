using System.Xml.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace MeetupEvents
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // var options = new MeetupEventOptions
            //     {DefaultCapacity = Configuration.GetValue("MeetupEventOptions:DefaultCapacity", 100)};
            // services.AddSingleton(options);

            var connectionString = Configuration.GetConnectionString("MeetupEvents");
            services.AddDbContext<MeetupEventDbContext>(o => o.UseNpgsql(connectionString));

            services.Configure<MeetupEventOptions>(Configuration.GetSection("MeetupEventOptions"));
            // services.AddSingleton<MeetupEventDb>();
            services.AddScoped<MeetupEventPostgresDb>();
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