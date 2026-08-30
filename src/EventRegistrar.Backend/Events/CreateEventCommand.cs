namespace EventRegistrar.Backend.Events;

public class CreateEventCommand : IRequest
{
    public string Acronym { get; set; }
    public Guid? EventId_Predecessor { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool CopyAccessRights { get; set; }
    public bool CopyRegistrables { get; set; }
    public bool CopyAutoMailTemplates { get; set; }
    public bool CopyBulkMailTemplates { get; set; }
    public bool CopyConfigurations { get; set; }
    public bool CopyPricing { get; set; }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand>
{
    public Task Handle(CreateEventCommand command, CancellationToken cancellationToken)
    {
        // EventRegistrar is winding down - creation of new events has been disabled.
        // See https://www.stomper.ch for the successor product.
        throw new InvalidOperationException("EventRegistrar is winding down. Creating new events is disabled. Please switch to https://www.stomper.ch.");
    }
}
