using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Infrastructure.ServiceBus;

namespace EventRegistrar.Backend.Infrastructure;

public class CommitUnitOfWorkDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly DbContext _dbContext;
    private readonly CommandQueue _commandQueue;
    private readonly EventBus _eventBus;

    public CommitUnitOfWorkDecorator(DbContext dbContext,
                                     CommandQueue commandQueue,
                                     EventBus eventBus)
    {
        _dbContext = dbContext;
        _commandQueue = commandQueue;
        _eventBus = eventBus;
    }

    public async Task<TResponse> Handle(TRequest request,
                                        RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
    {
        var response = await next();
        _dbContext.ChangeTracker.DetectChanges();
        if (_dbContext.ChangeTracker.HasChanges())
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // "transaction": only release messages to event bus if db commit succeeds
        await _commandQueue.Release();
        _eventBus.Release();

        return response;
    }
}