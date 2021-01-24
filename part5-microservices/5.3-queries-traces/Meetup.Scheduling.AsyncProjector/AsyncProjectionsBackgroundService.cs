using System.Threading;
using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.AsyncProjector
{
    public class AsyncProjectionsBackgroundService : BackgroundService
    {
        private readonly IDocumentStore            Store;
        private readonly ILogger<AsyncProjectionsBackgroundService> Logger;

        public AsyncProjectionsBackgroundService(IDocumentStore store, ILogger<AsyncProjectionsBackgroundService> logger)
        {
            Store  = store;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var daemon = Store.BuildProjectionDaemon();
            daemon.StartAll();
            await daemon.WaitForNonStaleResults(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}