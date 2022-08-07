using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

public class ReadModelUpdated : DomainEvent
{
    public string QueryName { get; set; } = null!;
    public Guid? RowId { get; set; }
}