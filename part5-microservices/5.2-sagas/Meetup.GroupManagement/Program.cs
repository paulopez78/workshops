using System.Threading.Tasks;
using Meetup.GroupManagement.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Meetup.GroupManagement
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using var scope    = host.Services.CreateScope();
            var       services = scope.ServiceProvider;
            var       context  = services.GetRequiredService<MeetupGroupManagementDbContext>();

            if (!context.Database.EnsureCreated())
                context.Database.Migrate();

            return host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
    }
}