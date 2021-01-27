using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MeetupEvents.Infrastructure
{
    public class ExceptionLoggingMiddleware<T> : IApplicationService
    {
        readonly IApplicationService AppService;
        readonly ILogger<T>          Logger;

        public ExceptionLoggingMiddleware(
            IApplicationService applicationService,
            ILogger<T> logger)
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