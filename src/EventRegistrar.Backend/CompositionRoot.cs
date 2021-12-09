using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using MediatR;
using SimpleInjector;

namespace EventRegistrar.Backend;

public class CompositionRoot
{
    public static void RegisterTypes(Container container)
    {
        var assemblies = new[] { typeof(SimpleInjectorExtensions).Assembly };

        container.Register(typeof(IRequestHandler<>), assemblies);
        container.Register(typeof(IRequestHandler<,>), assemblies);
        container.Collection.Register(typeof(IEventToCommandTranslation<>), assemblies);

        container.RegisterSingleton<IMediator, Mediator>();
        container.Register(() => new ServiceFactory(container.GetInstance), Lifestyle.Singleton);

        container.Collection.Register(typeof(IPipelineBehavior<,>), new[]
                                                                    {
                                                                        typeof(ExtractEventIdDecorator<,>),
                                                                        typeof(AuthorizationDecorator<,>),
                                                                        typeof(CommitUnitOfWorkDecorator<,>)
                                                                    });

        container.Register(typeof(IQueryable<>), typeof(Queryable<>));
        container.Register(typeof(IRepository<>), typeof(Repository<>));

        //var dbOptions = new DbContextOptionsBuilder<EventRegistratorDbContext>();
        //dbOptions.UseInMemoryDatabase("InMemoryDb");
        //container.RegisterInstance(dbOptions.Options);
        container.Register<DbContext, EventRegistratorDbContext>();

        container.Register(() =>
            new AuthenticatedUserId(container.GetInstance<IAuthenticatedUserProvider>()
                                             .GetAuthenticatedUserId()
                                             .Result));
        container.Register(() => container.GetInstance<IAuthenticatedUserProvider>().GetAuthenticatedUser());

        container.Register<IEventAcronymResolver, EventAcronymResolver>();
        container.Register<IAuthorizationChecker, AuthorizationChecker>();
        container.Register<IAuthenticatedUserProvider, AuthenticatedUserProvider>();
        container.Register<IRightsOfEventRoleProvider, RightsOfEventRoleProvider>();

        //container.Register(() => container.GetInstance<IHttpContextAccessor>().HttpContext?.Request ?? new DefaultHttpRequest(new DefaultHttpContext()));

        var assembly = typeof(CompositionRoot).Assembly;

        // Configuration
        container.Register<ConfigurationResolver>();
        var defaultConfigItemTypes = container.GetTypesToRegister<IDefaultConfigurationItem>(assembly).ToList();
        container.Collection.Register<IDefaultConfigurationItem>(defaultConfigItemTypes);

        var domainEventTypes = container.GetTypesToRegister<DomainEvent>(assembly);
        container.RegisterSingleton(() => new DomainEventCatalog(domainEventTypes));

        var configTypes = container.GetTypesToRegister<IConfigurationItem>(assembly)
                                   .Except(defaultConfigItemTypes);
        foreach (var configType in configTypes)
            container.Register(configType,
                () => container.GetInstance<ConfigurationResolver>().GetConfigurationTypeless(configType));

        var serviceBusConsumers = container.GetTypesToRegister<IQueueBoundMessage>(assembly)
                                           .Select(type => new ServiceBusConsumer
                                                           {
                                                               QueueName =
                                                                   ((IQueueBoundMessage)Activator.CreateInstance(type))
                                                                   .QueueName,
                                                               RequestType = type
                                                           })
                                           .ToList();
        container.RegisterInstance<IEnumerable<ServiceBusConsumer>>(serviceBusConsumers);
        container.RegisterSingleton<MessageQueueReceiver>();
        container.Register<ServiceBusClient>();
        container.Register<IEventBus, EventBus>();
        container.Register<SourceQueueProvider>();
        container.Register(typeof(IEventToUserTranslation<>), assembly);
        container.Verify();

        container.GetInstance<MessageQueueReceiver>().RegisterMessageHandlers();
    }
}