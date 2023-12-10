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
        var data = await mailTemplates.AsTracking()
                                      .Where(mtp => mtp.Id == command.MailTemplateId
                                                 && mtp.EventId == command.EventId)
                                      .Select(mtp => new
                                                     {
                                                         Template = mtp,
                                                         EventState = mtp.Event!.State,
                                                         MailCount = mtp.Mails!.Count
                                                     })
                                      .FirstAsync(cancellationToken);
        if (data.EventState != RegistrationForms.EventState.Setup || data.MailCount > 0)
        {
            throw new Exception($"To delete a template, event must be in state Setup, but it is in state {data.EventState}");
        }

        mailTemplates.Remove(data.Template);
    }
}