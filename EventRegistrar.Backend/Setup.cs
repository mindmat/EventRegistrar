﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Registrations.Register;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SimpleInjector;

namespace EventRegistrar.Backend
{
    public class Setup
    {
        public static void RegisterTypes(Container container)
        {
            var assemblies = new[] { typeof(SimpleInjectorExtensions).Assembly };

            container.Register(typeof(IRequestHandler<>), assemblies);
            container.Register(typeof(IRequestHandler<,>), assemblies);

            container.RegisterSingleton<IMediator, Mediator>();
            container.Register(() => new ServiceFactory(container.GetInstance), Lifestyle.Singleton);

            container.Collection.Register(typeof(IPipelineBehavior<,>), new[]
            {
                typeof(AuthorizationDecorator<,>),
                typeof(CommitUnitOfWorkDecorator<,>)
            });

            container.RegisterConditional(
                typeof(ILogger),
                c => typeof(Logger<>).MakeGenericType(c.Consumer.ImplementationType),
                Lifestyle.Singleton,
                c => true);

            container.Register(typeof(IQueryable<>), typeof(Queryable<>));
            container.Register(typeof(IRepository<>), typeof(Repository<>));

            //var dbOptions = new DbContextOptionsBuilder<EventRegistratorDbContext>();
            //dbOptions.UseInMemoryDatabase("InMemoryDb");
            //container.RegisterInstance(dbOptions.Options);
            container.Register<DbContext, EventRegistratorDbContext>();

            container.Register(() => new AuthenticatedUserId(container.GetInstance<IAuthenticatedUserProvider>().GetAuthenticatedUserId().Result));
            container.Register(() => container.GetInstance<IAuthenticatedUserProvider>().GetAuthenticatedUser());

            container.Register<IEventAcronymResolver, EventAcronymResolver>();
            container.Register<IAuthorizationChecker, AuthorizationChecker>();
            container.Register<IAuthenticatedUserProvider, AuthenticatedUserProvider>();
            container.Register<IRightsOfEventRoleProvider, RightsOfEventRoleProvider>();

            var assembly = typeof(Setup).Assembly;

            // first version: only use default config
            var configItemTypes = container.GetTypesToRegister<IDefaultConfigurationItem>(assembly);
            foreach (var configItemType in configItemTypes)
            {
                container.Register(configItemType.BaseType, configItemType);
            }

            var serviceBusConsumers = container.GetTypesToRegister<IRequest>(assembly)
                .Select(type => new
                {
                    RequestType = type,
                    Attribute = type.GetCustomAttribute<ProcessQueueMessageAttribute>()
                })
                .Where(tmp => tmp.Attribute != null)
                .Select(tmp => new ServiceBusConsumer
                {
                    RequestType = tmp.RequestType,
                    QueueName = tmp.Attribute.QueueName
                })
                .ToList();

            container.RegisterInstance<IEnumerable<ServiceBusConsumer>>(serviceBusConsumers);
            container.RegisterSingleton<MessageQueueReceiver>();

            container.Verify();

            container.GetInstance<MessageQueueReceiver>().RegisterMessageHandlers();
        }
    }
}