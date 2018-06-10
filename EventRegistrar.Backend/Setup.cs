using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SimpleInjector;
using System.Linq;

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
            container.Collection.Register(typeof(IPipelineBehavior<,>), assemblies);
            //container.Collection.Register(typeof(IRequestPreProcessor<>), new[] { typeof(GenericRequestPreProcessor<>) });
            //container.Collection.Register(typeof(IRequestPostProcessor<,>), new[] { typeof(GenericRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>) });

            container.Register(typeof(IQueryable<>), typeof(Queryable<>));
            container.Register<DbContext, EventRegistratorDbContext>();
        }
    }
}