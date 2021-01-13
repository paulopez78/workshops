using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Meetup.Scheduling.Infrastructure
{
    public static class DomainEventsDispatcherExtensions
    {
        public static IServiceCollection AddDomainEventsDispatcher(this IServiceCollection serviceCollection, Type type)
        {
            var registry = new DomainEventHandlerRegistry();
            RegisterHandlers(type.Assembly);

            serviceCollection.AddSingleton(registry);
            serviceCollection.AddScoped<DomainEventsDispatcher>();
            return serviceCollection;

            void RegisterHandlers(params Assembly[] assemblies)
            {
                foreach (var (message, wrapper) in GetTypesFromAssembly(typeof(IDomainEventHandler<>)))
                {
                    registry.Add(message, wrapper);
                    serviceCollection.AddScoped(wrapper);
                }

                IEnumerable<(Type message, Type wrapper)> GetTypesFromAssembly(Type interfaceType) =>
                    from ti in assemblies.SelectMany(a => a.DefinedTypes)
                    where ti.IsClass && !ti.IsAbstract && !ti.IsInterface
                    from i in ti.ImplementedInterfaces
                    where i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == interfaceType
                    select
                    (
                        messageType: i.GenericTypeArguments.First(),
                        wrapperType: ti.AsType()
                    );
            }
        }
    }

    public interface IDomainEventHandler<TMessage>
    {
        Task Handle(TMessage message);
    }

    public class DomainEventsDispatcher
    {
        readonly DomainEventHandlerRegistry Registry;

        public DomainEventsDispatcher(DomainEventHandlerRegistry registry)
        {
            Registry = registry;
        }

        public async Task Publish(IServiceProvider serviceProvider, object domainEvent)
        {
            if (Registry.TryGetValue(domainEvent.GetType(), out var handlers))
            {
                foreach (var handler in handlers.Select(serviceProvider.GetRequiredService))
                {
                    await ((dynamic) handler).Handle((dynamic) domainEvent);
                }
            }
        }
    }

    public class DomainEventHandlerRegistry : Dictionary<Type, List<Type>>
    {
        public void Add(Type domainEvent, Type handlerType)
        {
            var observed = ContainsKey(domainEvent);
            if (!observed)
                Add(domainEvent, new List<Type> {handlerType});
            else
                this[domainEvent].Add(handlerType);
        }
    }
}