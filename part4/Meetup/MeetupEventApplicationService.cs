using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using static Meetup.Scheduling.Commands.V1;

namespace Meetup.Scheduling
{
    public class MeetupEventApplicationService
    {
        readonly IRepository MeetupEventRepository;
        readonly ILogger<MeetupEventApplicationService> Logger;

        public MeetupEventApplicationService(IRepository meetupEventRepository,
            ILogger<MeetupEventApplicationService> logger)
        {
            MeetupEventRepository = meetupEventRepository;
            Logger = logger;
        }

        public Task<Guid> Handle(object command)
            =>
                command switch
                {
                    Create cmd
                        => MeetupEventRepository.Save(new Domain.MeetupEvent(Guid.NewGuid(), cmd.Group, cmd.Title,
                            cmd.Capacity)),
                    Publish cmd
                        => Execute(
                            cmd.EventId,
                            entity => entity.Publish()
                        ),
                    Cancel cmd
                        => Execute(
                            cmd.EventId,
                            entity => entity.Cancel()
                        ),
                    IncreaseCapacity cmd
                        => Execute(
                            cmd.EventId,
                            entity => entity.IncreaseCapacity(cmd.Capacity)
                        ),
                    ReduceCapacity cmd
                        => Execute(
                            cmd.EventId,
                            entity => entity.ReduceCapacity(cmd.Capacity)
                        ),
                    AcceptInvitation cmd
                        => Execute(
                            cmd.EventId,
                            entity => entity.AcceptInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    DeclineInvitation cmd
                        => Execute(
                            cmd.EventId,
                            entity => entity.DeclineInvitation(cmd.UserId, DateTimeOffset.Now)
                        ),
                    _
                        => throw new ApplicationException("command handler not found")
                };

        async Task<Guid> Execute(Guid id, Action<Domain.MeetupEvent> action)
        {
            Random jitterer = new();
            var retries = 10;

            var retryPolicy = Policy
                .Handle<DbUpdateConcurrencyException>()
                .WaitAndRetryAsync(retries, _ => TimeSpan.FromMilliseconds(jitterer.Next(100, 250)),
                    (exception, retrycount) =>
                    {
                        Logger.LogError(exception, $"Concurrency exception, Retrying {retrycount} of {retries}");
                    }
                );

            return await retryPolicy.ExecuteAsync(Execute);

            async Task<Guid> Execute()
            {
                var entity = await MeetupEventRepository.Load(id);
                if (entity is null) throw new ApplicationException($"Entity not found {id}");

                action(entity);

                await MeetupEventRepository.Save(entity);

                return id;
            }
        }
    }
}