using EventRegistrar.Backend.Infrastructure.ServiceBus;

using MediatR;

namespace EventRegistrar.Backend.Infrastructure;

public class CommitUnitOfWorkDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly DbContext _dbContext;
    private readonly CommandQueue _commandQueue;

    public CommitUnitOfWorkDecorator(DbContext dbContext,
                                     CommandQueue commandQueue)
    {
        _dbContext = dbContext;
        _commandQueue = commandQueue;
    }

    public async Task<TResponse> Handle(TRequest request,
                                        CancellationToken cancellationToken,
                                        RequestHandlerDelegate<TResponse> next)
    {
        var response = await next();
        if (_dbContext.ChangeTracker.HasChanges())
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // "transaction": only release messages to event bus if db commit succeeds
        await _commandQueue.Release();

        return response;
    }
}