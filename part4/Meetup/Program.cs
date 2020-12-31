using Meetup.Scheduling;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

CreateHostBuilder(args).Run();

static IHost CreateHostBuilder(string[] args)
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
        .Build();

    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MeetupDbContext>();
    if (!context.Database.EnsureCreated()) context.Database.Migrate();

    return host;
}