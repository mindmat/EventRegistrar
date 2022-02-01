using EventRegistrar.Backend.Infrastructure.ServiceBus;

using MediatR;

namespace EventRegistrar.Backend.Infrastructure;

public class CommitUnitOfWorkDecorator<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly DbContext _dbContext;
    private readonly ServiceBusClient _serviceBusClient;

    public CommitUnitOfWorkDecorator(DbContext dbContext,
                                     ServiceBusClient serviceBusClient)
    {
        _dbContext = dbContext;
        _serviceBusClient = serviceBusClient;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
                                        RequestHandlerDelegate<TResponse> next)
    {
        var response = await next();
        if (_dbContext.ChangeTracker.HasChanges())
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // "transaction": only release messages to event bus if db commit succeeds
        await _serviceBusClient.Release();

        return response;
    }
}