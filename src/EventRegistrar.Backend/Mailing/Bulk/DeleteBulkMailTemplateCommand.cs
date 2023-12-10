using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class DeleteBulkMailTemplateCommand : IEventBoundRequest, IRequest
{
    public Guid EventId { get; set; }
    public string? BulkMailKey { get; set; }
}

public class DeleteBulkMailTemplateCommandHandler(IRepository<BulkMailTemplate> mailTemplates,
                                                  ChangeTrigger changeTrigger) : IRequestHandler<DeleteBulkMailTemplateCommand>
{
    public async Task Handle(DeleteBulkMailTemplateCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.BulkMailKey))
        {
            return;
        }

        var data = await mailTemplates.AsTracking()
                                      .Where(mtp => mtp.BulkMailKey == command.BulkMailKey
                                                 && mtp.EventId == command.EventId)
                                      .Select(mtp => new
                                                     {
                                                         Template = mtp,
                                                         EventState = mtp.Event!.State,
                                                         MailCount = mtp.Mails!.Count
                                                     })
                                      .ToListAsync(cancellationToken);

        if (data.Exists(dta => dta.EventState != RegistrationForms.EventState.Setup && dta.MailCount > 0))
        {
            throw new InvalidOperationException($"To delete a template, event must be in state Setup, but it is in state {data.First().EventState}");
        }

        foreach (var item in data)
        {
            mailTemplates.Remove(item.Template);
        }
    }
}