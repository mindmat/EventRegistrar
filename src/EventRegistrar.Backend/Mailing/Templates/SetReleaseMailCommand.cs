namespace EventRegistrar.Backend.Mailing.Templates;

public class SetReleaseMailCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public MailType Type { get; set; }
    public bool ReleaseImmediately { get; set; }
}

public class SetReleaseMailCommandHandler : IRequestHandler<SetReleaseMailCommand>
{
    private readonly IRepository<AutoMailTemplate> _templates;

    public SetReleaseMailCommandHandler(IRepository<AutoMailTemplate> templates)
    {
        _templates = templates;
    }

    public async Task<Unit> Handle(SetReleaseMailCommand command, CancellationToken cancellationToken)
    {
        var templates = await _templates.Where(amt => amt.EventId == command.EventId
                                                   && amt.Type == command.Type)
                                        .ToListAsync(cancellationToken);
        foreach (var template in templates)
        {
            template.ReleaseImmediately = command.ReleaseImmediately;
        }

        return Unit.Value;
    }
}