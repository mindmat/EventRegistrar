namespace EventRegistrar.Backend.Infrastructure.Mediator;

public struct SerializedJson<TContent> : ISerializedJson
    where TContent : class?
{
    public SerializedJson(string json)
    {
        Content = json;
    }

    public string Content { get; }
    public Type ContentType => typeof(TContent);
}

public interface ISerializedJson
{
    public Type ContentType { get; }
    public string Content { get; }
}