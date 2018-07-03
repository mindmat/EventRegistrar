using System.Linq;
using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Events;
using EventRegistrar.Backend.Events.UsersInEvents;
using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
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
            //container.Collection.Register(typeof(IRequestPreProcessor<>), new[] { typeof(RequestPreProcessorBehavior<,>) });
            //container.Collection.Register(typeof(IRequestPostProcessor<,>), new[] { typeof(GenericRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>) });

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
        }
    }
}