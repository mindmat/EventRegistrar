using System.Reflection;

using Azure.Identity;
using Azure.Messaging.ServiceBus;

using EventRegistrar.Backend.Authentication;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.Context;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Hosting;
using EventRegistrar.Backend.Infrastructure.AuditLog;
using EventRegistrar.Backend.Infrastructure.Configuration;
using EventRegistrar.Backend.Infrastructure.DataAccess.DirtyTags;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ErrorHandling;
using EventRegistrar.Backend.Infrastructure.Mediator;
using EventRegistrar.Backend.Infrastructure.MenuNodes;
using EventRegistrar.Backend.Infrastructure.ReadableIds;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Mailing.Templates.Validation;

using Microsoft.Data.SqlClient;

using SimpleInjector;

namespace EventRegistrar.Backend.Infrastructure.Startup;

public static class ContainerExtensions
{
    public static void RegisterMediatr(this Container container,
                                       Assembly[] assemblies,
                                       bool skipAuditLog = false)
    {
        var requestQueryTypes = container.GetTypesToRegister(typeof(IRequestHandler<,>), assemblies);
        var requestCommandTypes = container.GetTypesToRegister(typeof(IRequestHandler<>), assemblies);
        container.RegisterSingleton(() => new RequestRegistry(requestQueryTypes, requestCommandTypes));

        container.Register(typeof(IRequestHandler<>), assemblies);
        container.Register(typeof(IRequestHandler<,>), assemblies);
        container.Collection.Register(typeof(IEventToCommandTranslation<>), assemblies);

        container.Register<IMediator>(() => new MediatR.Mediator(new SimpleInjectorServiceProvider(container)), Lifestyle.Singleton);

        container.Register<EventContext>();

        var pipelineBehaviours = skipAuditLog
                                     ? new[]
                                       {
                                           typeof(ExtractEventIdDecorator<,>),
                                           typeof(CommitUnitOfWorkDecorator<,>)
                                       }
                                     : new[]
                                       {
                                           typeof(ExtractEventIdDecorator<,>),
                                           typeof(AuditLogger<,>),
                                           typeof(CommitUnitOfWorkDecorator<,>)
                                       };
        container.Collection.Register(typeof(IPipelineBehavior<,>), pipelineBehaviours);
    }

    public static void RegisterDataAccessToSqlServer(this Container container,
                                                     Assembly[] assemblies,
                                                     string? connectionString)
    {
        if (connectionString == null)
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
        SetDbOptions(optionsBuilder, connectionString);
        container.RegisterInstance(optionsBuilder);
        container.RegisterInstance(optionsBuilder.Options);

        var optionsBuilderAuditLog = new DbContextOptionsBuilder<AuditLogDbContext>();
        SetDbOptions(optionsBuilderAuditLog, connectionString);

        var dbConnection = new SqlConnection(connectionString);
        container.RegisterInstance(dbConnection);

        container.RegisterDataAccess(assemblies, optionsBuilderAuditLog.Options);
    }


    public static void RegisterDataAccess(this Container container,
                                          Assembly[] assemblies,
                                          DbContextOptions<AuditLogDbContext>? auditLogDbOptions = null)
    {
        container.Register<IHttpContextAccessor, HttpContextContainer>();

        container.Register(typeof(IQueryable<>), typeof(Queryable<>));
        container.Register(typeof(IRepository<>), typeof(Repository<>));

        container.Register<DbContext, EventRegistratorDbContext>();
        if (auditLogDbOptions != null)
        {
            container.Register(() => new AuditLogDbContext(auditLogDbOptions));
        }
    }

    public static void RegisterIamWithAuth0(this Container container)
    {
        container.Register<IIdentityProvider, Auth0IdentityProvider>();
        container.RegisterSingleton<Auth0TokenProvider>();

        container.RegisterIam();
    }

    public static void RegisterIam(this Container container)
    {
        container.Register(() => new AuthenticatedUserId(container.GetInstance<IAuthenticatedUserProvider>()
                                                                  .GetAuthenticatedUserId()
                                                                  .Result));
        container.Register(() => container.GetInstance<IAuthenticatedUserProvider>()
                                          .GetAuthenticatedUser());
        container.Register<IAuthorizationChecker, AuthorizationChecker>();
        container.Register<IAuthenticatedUserProvider, AuthenticatedUserProvider>();
        container.Register<IRightsOfEventRoleProvider, RightsOfEventRoleProvider>();
    }

    public static void RegisterErrorHandling(this Container container, Assembly[] assemblies)
    {
        container.RegisterSingleton<ExceptionTranslator>();
        container.Collection.Register<IExceptionTranslation>(assemblies);
    }

    public static void RegisterConfiguration(this Container container, Assembly[] assemblies)
    {
        container.Register<ConfigurationRegistry>();
        var defaultConfigItemTypes = container.GetTypesToRegister<IDefaultConfigurationItem>(assemblies)
                                              .ToList();
        container.Collection.Register<IDefaultConfigurationItem>(defaultConfigItemTypes, Lifestyle.Singleton);

        var configTypes = container.GetTypesToRegister<IConfigurationItem>(assemblies)
                                   .Except(defaultConfigItemTypes);
        foreach (var configType in configTypes)
        {
            container.Register(configType, () => container.GetInstance<ConfigurationRegistry>().GetConfigurationTypeless(configType));
            // add the possibility to inject FeatureConfigurations to singletons
            container.RegisterSingleton(typeof(SingletonConfigurationFeature<>).MakeGenericType(configType),
                                        () => GetSingletonConfig(container, configType));
        }
    }

    public static void RegisterEventBus(this Container container, Assembly[] assemblies)
    {
        var domainEventTypes = container.GetTypesToRegister<DomainEvent>(assemblies);
        container.RegisterSingleton(() => new DomainEventCatalog(domainEventTypes));
        container.Register<ChangeTrigger>();
        container.Register<IEventBus, EventBus>();
        container.Register<SourceQueueProvider>();
        container.Register(typeof(IEventToUserTranslation<>), assemblies);
    }

    public static void RegisterAzureServiceBus(this Container container,
                                               string? connectionString,
                                               string? @namespace)
    {
        container.RegisterSingleton(() => connectionString != null
                                              ? new ServiceBusClient(connectionString)
                                              : new ServiceBusClient(@namespace, new DefaultAzureCredential()));
        container.RegisterSingleton(() => container.GetInstance<ServiceBusClient>().CreateSender(CommandQueue.CommandQueueName));
        container.Register<ICommandQueue, CommandQueue>();
        container.RegisterSingleton<MessageQueueReceiver>();
    }

    public static void RegisterMisc(this Container container, Assembly[] assemblies)
    {
        container.Collection.Register(typeof(IReadModelCalculator), assemblies);
        container.Collection.Register(typeof(IMenuNodeCalculator), assemblies);

        container.Register<IEventAcronymResolver, EventAcronymResolver>();

        container.RegisterSingleton<IDateTimeProvider, DateTimeProvider>();
        container.Register<RequestDateTimeProvider>();

        container.Collection.Register(typeof(IDirtySegment), assemblies);
        container.Register<DirtyTagger>();
        container.Register<HostingMappingReader>();

        container.RegisterSingleton<SecretReader>();
        container.Register<ReadableIdProvider>();

        container.Collection.Register<IAutoMailTemplateExpectedPlaceholders>(assemblies, Lifestyle.Singleton);
    }

    static void SetDbOptions(DbContextOptionsBuilder o, string connectionString)
    {
        o.UseSqlServer(connectionString, sqlBuilder => { sqlBuilder.EnableRetryOnFailure(); })
         .EnableSensitiveDataLogging();
    }


    static object GetSingletonConfig(Container container, Type featureConfigType)
    {
        using (new EnsureExecutionScope(container))
        {
            var singletonConfigurationFeatureType = typeof(SingletonConfigurationFeature<>).MakeGenericType(featureConfigType);
            var constructor = singletonConfigurationFeatureType.GetConstructor([featureConfigType]);
            return constructor?.Invoke([container.GetInstance(featureConfigType)])!;
        }
    }
}