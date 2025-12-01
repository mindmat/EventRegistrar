using EventRegistrar.Backend.Infrastructure;
using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;
using EventRegistrar.Backend.Mailing.Templates;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class UpdateBulkMailTemplateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid TemplateId { get; set; }
    public string? SenderMail { get; set; }
    public string? SenderName { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
    public IEnumerable<MailingAudience>? Audiences { get; set; }
    public IEnumerable<Guid>? RegistrableIds { get; set; }
    public bool AddIcs { get; set; }
}

public class UpdateBulkMailTemplateCommandHandler(IRepository<BulkMailTemplate> bulkMailTemplates,
                                                  IEventBus eventBus)
    : IRequestHandler<UpdateBulkMailTemplateCommand>
{
    public async Task Handle(UpdateBulkMailTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await bulkMailTemplates.AsTracking()
                                              .FirstAsync(mtp => mtp.EventId == command.EventId
                                                              && mtp.Id == command.TemplateId,
                                                          cancellationToken);
        template.SenderMail = command.SenderMail;
        template.SenderName = command.SenderName;
        template.Subject = command.Subject;
        template.AddIcs = command.AddIcs;
        if (!string.IsNullOrWhiteSpace(command.ContentHtml))
        {
            template.ContentHtml = command.ContentHtml;
        }

        template.MailingAudience = command.Audiences.ConvertToFlags();
        template.RegistrableIds = command.RegistrableIds?.ToList();
        eventBus.Publish(new QueryChanged
                         {
                             QueryName = nameof(BulkMailPreviewQuery),
                             EventId = command.EventId,
                             RowId = template.Id
                         });
    }
}