namespace EventRegistrar.Backend.Authorization;

public interface IEventBoundRequest
{
    Guid EventId { get; }
}