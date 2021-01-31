using System;
using System.Threading.Tasks;
using MeetupEvents.Domain;

namespace MeetupEvents.Infrastructure
{
    public record CommandResult (Guid Id, bool OkResult);

    public interface IApplicationService
    {
        Task<CommandResult> HandleCommand(Guid id, object command);
    }

    public delegate Task<Guid?> GetMapId(Guid id);

    public abstract class ApplicationService<TAggregate> : IApplicationService where TAggregate : Aggregate, new()
    {
        readonly Repository<TAggregate> Repository;

        protected ApplicationService(Repository<TAggregate> repository)
            => Repository = repository;

        public abstract Task<CommandResult> HandleCommand(Guid id, object command);

        protected async Task<CommandResult> Handle(Guid id, Action<TAggregate> commandHandler)
        {
            // load entity
            var aggregate = await Repository.Load(id);
            if (aggregate is null) return new(id, false);

            // execute business logic
            commandHandler(aggregate);

            // commit transaction
            await Repository.SaveChanges(aggregate);
            
            return new(id, true);
        }

        protected async Task<CommandResult> HandleCreate(Guid id, Action<TAggregate> commandHandler)
        {
            // check if already exists
            var aggregate = await Repository.Load(id);
            if (aggregate is not null)
                throw new InvalidOperationException($"{typeof(TAggregate).Name} {id} already exists");

            // execute business logic
            aggregate = new TAggregate();
            commandHandler(aggregate);
            await Repository.Add(aggregate);

            // commit transaction
            await Repository.SaveChanges(aggregate);
            return new(id, true);
        }
    }
}