using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Templates;

public class UpdateAutoMailTemplateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid TemplateId { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
}

public class UpdateAutoMailTemplateCommandHandler(IRepository<AutoMailTemplate> autoMailTemplates,
                                                  IEventBus eventBus)
    : IRequestHandler<UpdateAutoMailTemplateCommand>
{
    public async Task Handle(UpdateAutoMailTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await autoMailTemplates.AsTracking()
                                              .FirstAsync(mtp => mtp.EventId == command.EventId
                                                              && mtp.Id == command.TemplateId,
                                                          cancellationToken);
        template.Subject = command.Subject;
        if (!string.IsNullOrWhiteSpace(command.ContentHtml))
        {
            template.ContentHtml = command.ContentHtml;
        }

        eventBus.Publish(new QueryChanged
                         {
                             QueryName = nameof(MailTemplatePreviewQuery),
                             EventId = command.EventId,
                             RowId = template.Id
                         });
    }
}