using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace;
using Prometheus;

namespace Meetup.Scheduling.Queries
{
    public class Startup
    {
        public static string ApplicationKey = "meetup_scheduling";

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenTelemetryTracing(b =>
                b.AddAspNetCoreInstrumentation()
                    .AddJaegerExporter(o => o.ServiceName = ApplicationKey)
                    .AddZipkinExporter(o => o.ServiceName = ApplicationKey)
            );
            services.AddControllers();
            services.AddMarten(cfg =>
            {
                cfg.Connection(Configuration.GetConnectionString("MeetupScheduling"));
                cfg.DatabaseSchemaName        = "scheduling";
                cfg.Events.DatabaseSchemaName = "scheduling";
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Meetup Scheduling Queries", Version = "v1"});
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "meetup scheduling queries v1"));
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