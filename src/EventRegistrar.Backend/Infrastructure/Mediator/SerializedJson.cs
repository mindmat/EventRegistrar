namespace EventRegistrar.Backend.Infrastructure.Mediator;

public struct SerializedJson<TContent>(string json) : ISerializedJson
    where TContent : class?
{
    public string Content { get; } = json;
    public Type ContentType => typeof(TContent);
}

public interface ISerializedJson
{
    public Type ContentType { get; }
    public string Content { get; }
}