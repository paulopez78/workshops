using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Meetup.Scheduling.Test
{
    public class WebApiFixture : WebApplicationFactory<Startup>
    {
        public ITestOutputHelper Output { get; set; }

        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = base.CreateHostBuilder();
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddXUnit(Output);
            });

            return builder;
        }
    }
}