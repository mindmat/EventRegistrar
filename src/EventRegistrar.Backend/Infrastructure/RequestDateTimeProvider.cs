namespace EventRegistrar.Backend.Infrastructure;


public class RequestDateTimeProvider(IDateTimeProvider dateTimeProvider)
{
    public DateTimeOffset RequestNow { get; init; } = dateTimeProvider.Now;
}