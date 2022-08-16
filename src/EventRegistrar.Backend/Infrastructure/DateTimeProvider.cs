namespace EventRegistrar.Backend.Infrastructure;

public interface IDateTimeProvider
{
    public DateTimeOffset Now { get; }
}

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}