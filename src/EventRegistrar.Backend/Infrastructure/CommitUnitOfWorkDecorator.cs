using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Infrastructure;

public class CommitUnitOfWorkDecorator<TRequest, TResponse>(DbContext dbContext,
                                                            CommandQueue commandQueue,
                                                            EventBus eventBus)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request,
                                        RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
    {
        try
        {
            var response = await next();
            dbContext.ChangeTracker.DetectChanges();
            if (dbContext.ChangeTracker.HasChanges())
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // "transaction": only release messages to event bus if db commit succeeds
            await commandQueue.Release(true);
            eventBus.Release(true);
            return response;
        }
        catch
        {
            await commandQueue.Release(false);
            eventBus.Release(false);
            throw;
        }
    }
}