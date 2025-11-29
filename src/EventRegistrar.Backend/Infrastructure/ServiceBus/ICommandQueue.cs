namespace EventRegistrar.Backend.Infrastructure.ServiceBus
{
    public interface ICommandQueue
    {
        Task Release(bool dbCommitSucceeded);

        void EnqueueCommand<T>(T command,
                               bool publishEvenWhenDbCommitFails = false,
                               TimeSpan? delay = null)
            where T : IRequest;
    }
}