using System.Threading;
using System.Threading.Tasks;
using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.Queries
{
    public class AsyncProjections : BackgroundService
    {
        private readonly IDocumentStore            Store;
        private readonly ILogger<AsyncProjections> Logger;

        public AsyncProjections(IDocumentStore store, ILogger<AsyncProjections> logger)
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