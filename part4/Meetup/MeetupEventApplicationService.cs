using System;
using System.Linq;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using static Meetup.Scheduling.Commands.V1;

namespace Meetup.Scheduling
{
    public class MeetupEventApplicationService
    {
        readonly IRepository                            MeetupEventRepository;
        readonly ILogger<MeetupEventApplicationService> Logger;

        public MeetupEventApplicationService(IRepository meetupEventRepository,
            ILogger<MeetupEventApplicationService> logger)
        {
            MeetupEventRepository = meetupEventRepository;
            Logger                = logger;
        }

        public Task<Guid> Handle(object command)
            =>
                command switch
                {
                    Create cmd
                        => MeetupEventRepository.Save(new MeetupEvent(Guid.NewGuid(), cmd.Group, cmd.Title,
                            cmd.Capacity)),
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
                    IncreaseCapacity cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.IncreaseCapacity(cmd.Capacity)
                        ),
                    ReduceCapacity cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.ReduceCapacity(cmd.Capacity)
                        ),
                    AcceptInvitation cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.AcceptInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    DeclineInvitation cmd
                        => Handle(
                            cmd.EventId,
                            entity => entity.DeclineInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };

        async Task<Guid> Handle(Guid id, Action<MeetupEvent> action)
        {
            var entity = await MeetupEventRepository.Load(id);
            if (entity is null) throw new ApplicationException($"Entity not found {id}");

            action(entity);

            await MeetupEventRepository.Save(entity);

            return id;
        }

        Task<Guid> HandleWithRetry(Guid id, Action<MeetupEvent> action)
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
                        var entry = ex.Entries.FirstOrDefault(x => x.Entity is MeetupEvent);
                        entry?.OriginalValues.SetValues(entry.GetDatabaseValues());
                    });
        }
    }
}