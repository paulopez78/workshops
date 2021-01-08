using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Meetup.Scheduling.Application
{
    public record CommandResult(Guid EventId);

    public interface IApplicationService
    {
        Task<CommandResult> Handle(object command);
    }

    public static class ApplicationServiceExtensions
    {
        public static IApplicationService Build(this IApplicationService applicationService, ILogger logger) =>
            new ExceptionHandlingLoggerMiddleware(
                new RetryConcurrentUpdatesMiddleware(applicationService, logger), logger);

        public static async Task<IActionResult> HandleCommand(this IApplicationService applicationService,
            object command)
        {
            try
            {
                return new OkObjectResult(await applicationService.Handle(command));
            }
            catch (ApplicationException e)
            {
                return new BadRequestObjectResult(e.Message);
            }

            catch (DbUpdateConcurrencyException e)
            {
                return new ObjectResult(e.Message)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }
    }


    public class ExceptionHandlingLoggerMiddleware : IApplicationService
    {
        readonly ILogger             Logger;
        readonly IApplicationService ApplicationService;

        public ExceptionHandlingLoggerMiddleware(IApplicationService applicationService, ILogger logger)
        {
            ApplicationService = applicationService;
            Logger             = logger;
        }

        public async Task<CommandResult> Handle(object command)
        {
            try
            {
                Logger.LogDebug($"Executing {ApplicationService} command {command.GetType().FullName}");
                var result = await ApplicationService.Handle(command);
                Logger.LogDebug($"Executed {ApplicationService} command {command.GetType().FullName}");

                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e,
                    $"Error executing application service {ApplicationService} with command {command.GetType().FullName}");
                throw;
            }
        }
    }

    public class RetryConcurrentUpdatesMiddleware : IApplicationService
    {
        readonly ILogger             Logger;
        readonly IApplicationService ApplicationService;

        public RetryConcurrentUpdatesMiddleware(IApplicationService applicationService, ILogger logger)
        {
            ApplicationService = applicationService;
            Logger             = logger;
        }

        public Task<CommandResult> Handle(object command)
        {
            return RetryConcurrentUpdate().ExecuteAsync(() => ApplicationService.Handle(command));

            AsyncRetryPolicy RetryConcurrentUpdate(int retries = 3) => Policy
                .Handle<DbUpdateConcurrencyException>()
                // .WaitAndRetryAsync(retries, _ => TimeSpan.FromMilliseconds(jitterer.Next(0, 0)),
                .RetryAsync(retries,
                    (exception, retrycount) =>
                    {
                        Logger.LogError(exception, $"Concurrency exception, Retrying {retrycount} of {retries}");

                        //https://docs.microsoft.com/en-us/ef/core/saving/concurrency
                        if (exception is not DbUpdateConcurrencyException ex) return;

                        var entry = ex.Entries.FirstOrDefault();
                        entry?.OriginalValues.SetValues(entry.GetDatabaseValues());
                    });
        }
    }
}