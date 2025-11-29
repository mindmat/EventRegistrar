using System.Reflection;

using EventRegistrar.Backend.Infrastructure.AuditLog;
using EventRegistrar.Backend.Infrastructure.DataAccess;
using EventRegistrar.Backend.Infrastructure.ServiceBus;
using EventRegistrar.Backend.Infrastructure.Startup;

using MediatR;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using SimpleInjector;

namespace ApiTestInfrastructure;

public static class ContainerTestExtensions
{
    extension(Container container)
    {
        public void RegisterDataAccessToInMemoryDb(Assembly[] assemblies)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EventRegistratorDbContext>();
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            container.RegisterInstance(optionsBuilder);
            container.RegisterInstance(optionsBuilder.Options);

            container.RegisterDataAccess(assemblies);

            // dummy connection
            // used in ProcessRawRegistrationCommandHandler -> can not be tested against InMemoryDb
            container.RegisterInstance(new SqlConnection());
        }

        public void RegisterInMemoryQueue()
        {
            container.Register<ICommandQueue, InMemoryCommandQueue>();
        }
    }
}

public class InMemoryCommandQueue : ICommandQueue
{
    public Task Release(bool dbCommitSucceeded)
    {
        return Task.CompletedTask;
    }

    public void EnqueueCommand<T>(T command, bool publishEvenWhenDbCommitFails = false, TimeSpan? delay = null) where T : IRequest { }
}