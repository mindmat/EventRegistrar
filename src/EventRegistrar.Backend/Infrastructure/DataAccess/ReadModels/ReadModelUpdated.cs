namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class ReadModelUpdated
{
    public Guid EventId { get; set; }
    public string ReadModelName { get; set; } = null!;
    public Guid RowId { get; set; }
}