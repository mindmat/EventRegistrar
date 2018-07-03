namespace EventRegistrar.Backend.Authorization
{
    public interface IEventBoundRequest
    {
        string EventAcronym { get; }
    }
}