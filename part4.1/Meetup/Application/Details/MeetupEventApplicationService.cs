using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Meetup.Scheduling.Domain;
using Meetup.Scheduling.Infrastructure;
using static Meetup.Scheduling.Application.Details.Commands.V1;

namespace Meetup.Scheduling.Application.Details
{
    public class MeetupEventDetailsApplicationService: IApplicationService
    {
        readonly MeetupEventDetailsRepository                  MeetupEventRepository;
        readonly ILogger<MeetupEventDetailsApplicationService> Logger;

        public MeetupEventDetailsApplicationService(MeetupEventDetailsRepository meetupEventRepository,
            ILogger<MeetupEventDetailsApplicationService> logger)
        {
            MeetupEventRepository = meetupEventRepository;
            Logger                = logger;
        }

        public Task<Guid> Handle(object command)
            =>
                command switch
                {
                    Create cmd
                        => MeetupEventRepository.Save(new MeetupEventDetails(Guid.NewGuid(), cmd.Group, cmd.Title)),
                    UpdateDetails cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.UpdateDetails(cmd.Title)
                        ),
                    Publish cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.Publish()
                        ),
                    Cancel cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.Cancel()
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };

        async Task<Guid> Handle(Guid id, Action<MeetupEventDetails> action)
        {
            var entity = await MeetupEventRepository.Load(id);
            if (entity is null) throw new ApplicationException($"Entity not found {id}");

            action(entity);

            await MeetupEventRepository.Save(entity);

            return id;
        }

        Task<Guid> HandleWithRetry(Guid id, Action<MeetupEventDetails> action)
        {
            return RetryConcurrentUpdate().ExecuteAsync(() => Handle(id, action));

            AsyncRetryPolicy RetryConcurrentUpdate(int retries = 3) => Policy
                .Handle<DbUpdateConcurrencyException>()
                // .WaitAndRetryAsync(retries, _ => TimeSpan.FromMilliseconds(jitterer.Next(0, 0)),
                .RetryAsync(retries,
                    (exception, retrycount) =>
                    {
                        Logger.LogError(exception, $"Concurrency exception, Retrying {retrycount} of {retries}");

                        //https://docs.microsoft.com/en-us/ef/core/saving/concurrency
                        if (exception is not DbUpdateConcurrencyException ex) return;
                        var entry = ex.Entries.FirstOrDefault(x => x.Entity is MeetupEventDetails);
                        entry?.OriginalValues.SetValues(entry.GetDatabaseValues());
                    });
        }
    }
}