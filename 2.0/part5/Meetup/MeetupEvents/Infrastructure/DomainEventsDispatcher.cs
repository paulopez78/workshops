using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MeetupEvents.Infrastructure
{
    public static class DomainEventsDispatcherExtensions
    {
        public static IServiceCollection AddDomainEventsDispatcher(this IServiceCollection serviceCollection, Type type)
        {
            var registry = new DomainEventHandlerRegistry();
            RegisterHandlers(type.Assembly);

            serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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
        readonly IServiceProvider           ServiceProvider;
        readonly DomainEventHandlerRegistry Registry;

        public DomainEventsDispatcher(IHttpContextAccessor httpContextAccessor, DomainEventHandlerRegistry registry)
        {
            ServiceProvider = httpContextAccessor.HttpContext.RequestServices;
            Registry        = registry;
        }

        public async Task Publish(object domainEvent)
        {
            if (Registry.TryGetValue(domainEvent.GetType(), out var handlers))
            {
                foreach (var handler in handlers.Select(handlerType => ServiceProvider.GetRequiredService(handlerType)))
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