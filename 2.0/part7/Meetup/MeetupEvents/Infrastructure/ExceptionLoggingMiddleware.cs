using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Infrastructure
{
    public class ExceptionLoggingMiddleware<TLogger> : IApplicationService
    {
        readonly IApplicationService AppService;
        readonly ILogger<TLogger>    Logger;

        public ExceptionLoggingMiddleware(
            IApplicationService applicationService,
            ILogger<TLogger> logger)
        {
            AppService = applicationService;
            Logger     = logger;
        }

        public async Task<CommandResult> HandleCommand(Guid id, object command)
        {
            try
            {
                Logger.LogDebug($"Command {command.GetType().Name} executing for entity {id}");

                var result = await AppService.HandleCommand(id, command);

                Logger.LogDebug($"Command {command.GetType().Name} executed for entity {id}");
                return result;
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error executing command {command.GetType().Name} with entity {id} ");
                throw;
            }
        }
    }
}