namespace EventRegistrar.Backend.Mailing.Templates;

public class SetReleaseMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public MailType Type { get; set; }
    public bool ReleaseImmediately { get; set; }
}

public class SetReleaseMailCommandHandler(IRepository<AutoMailTemplate> _templates) : IRequestHandler<SetReleaseMailCommand>
{
    public async Task Handle(SetReleaseMailCommand command, CancellationToken cancellationToken)
    {
        var templates = await _templates.Where(amt => amt.EventId == command.EventId
                                                   && amt.Type == command.Type)
                                        .ToListAsync(cancellationToken);
        foreach (var template in templates)
        {
            template.ReleaseImmediately = command.ReleaseImmediately;
        }
    }
}