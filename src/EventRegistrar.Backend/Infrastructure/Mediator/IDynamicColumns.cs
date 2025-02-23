namespace EventRegistrar.Backend.Infrastructure.Mediator;

public interface IDynamicColumns
{
    IDictionary<string, string>? DynamicColumns { get; set; }
}