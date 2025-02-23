namespace EventRegistrar.Backend.Infrastructure.Mediator;

public class DownloadResult
{
    public required string ContentType { get; set; }
    public ReadOnlyMemory<byte> Content { get; set; }
}