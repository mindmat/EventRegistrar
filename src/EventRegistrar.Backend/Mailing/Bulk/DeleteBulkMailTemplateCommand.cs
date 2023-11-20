namespace EventRegistrar.Backend.Mailing.Bulk;

public class DeleteBulkMailTemplateCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
    public Guid MailTemplateId { get; set; }
}

public class DeleteBulkMailTemplateCommandHandler(IRepository<BulkMailTemplate> mailTemplates) : IRequestHandler<DeleteBulkMailTemplateCommand>
{
    public async Task Handle(DeleteBulkMailTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await mailTemplates.AsTracking()
                                          .Where(mtp => mtp.Id == command.MailTemplateId
                                                     && mtp.EventId == command.EventId)
                                          .Include(mtp => mtp.Event)
                                          .FirstAsync(cancellationToken);
        if (template.Event!.State != RegistrationForms.EventState.Setup)
        {
            throw new Exception($"To delete a template, event must be in state Setup, but it is in state {template.Event.State}");
        }

        mailTemplates.Remove(template);
    }
}