using System;
using System.Collections.Generic;
using static System.Linq.Enumerable;
using System.Threading.Tasks;
using MeetupEvents.Domain;

namespace MeetupEvents.Infrastructure
{
    public record CommandResult (Guid Id, IEnumerable<object> Changes);

    public interface IApplicationService
    {
        Task<CommandResult> HandleCommand(Guid id, object command);
    }

    public delegate Task<Guid?> GetMapId(Guid id);

    public abstract class ApplicationService<TAggregate> : IApplicationService
        where TAggregate : Aggregate, new()
    {
        readonly Repository<TAggregate> Repository;

        protected ApplicationService(Repository<TAggregate> repository)
            => Repository = repository;

        public abstract Task<CommandResult> HandleCommand(Guid id, object command);

        protected async Task<CommandResult> Handle(Guid id, Action<TAggregate> commandHandler)
        {
            // load entity
            var aggregate = await Repository.Load(id);
            if (aggregate is null) return new(id, Empty<object>());

            return await Commit(commandHandler, aggregate);
        }

        protected async Task<CommandResult> HandleCreate(Guid id, Action<TAggregate> commandHandler)
        {
            // check if already exists
            var aggregate = await Repository.Load(id);
            if (aggregate is not null)
                throw new InvalidOperationException($"{typeof(TAggregate).Name} {id} already exists");

            // execute business logic
            aggregate = new TAggregate();
            await Repository.Add(aggregate);

            return await Commit(commandHandler, aggregate);
        }

        async Task<CommandResult> Commit(Action<TAggregate> commandHandler, TAggregate aggregate)
        {
            commandHandler(aggregate);

            aggregate.IncreaseVersion();

            // commit transaction
            await Repository.SaveChanges();
            var result = new CommandResult(aggregate.Id, aggregate.Changes.ToList());

            aggregate.ClearChanges();
            return result;
        }
    }
}