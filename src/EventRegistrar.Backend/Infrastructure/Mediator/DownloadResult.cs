namespace EventRegistrar.Backend.Infrastructure.Mediator;

public class DownloadResult
{
    public required string ContentType { get; set; }
    public required ReadOnlyMemory<byte> Content { get; set; }
    public string? Filename { get; set; }
}