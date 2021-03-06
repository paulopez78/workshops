using System;
using System.Threading.Tasks;
using Meetup.Scheduling.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meetup.Scheduling.Application
{
    public record CommandResult(Guid EventId);

    public interface IApplicationService
    {
        Task<CommandResult> Handle(object command);
    }

    public static class ApplicationServiceExtensions
    {
        public static IApplicationService Build(this IApplicationService applicationService,
            MeetupSchedulingDbContext dbContext, ILogger logger) =>
            new ExceptionHandlingLoggerMiddleware(new TransactionMiddleware(applicationService, dbContext), logger);

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
            catch (ArgumentException e)
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

    public class TransactionMiddleware : IApplicationService
    {
        readonly IApplicationService       ApplicationService;
        readonly MeetupSchedulingDbContext DbContext;

        public TransactionMiddleware(IApplicationService applicationService, MeetupSchedulingDbContext dbContext)
        {
            ApplicationService = applicationService;
            DbContext          = dbContext;
        }

        public async Task<CommandResult> Handle(object command)
        {
            await using var transaction = await DbContext.Database.BeginTransactionAsync();

            var result = await ApplicationService.Handle(command);

            await DbContext.Database.CommitTransactionAsync();

            return result;
        }
    }
}