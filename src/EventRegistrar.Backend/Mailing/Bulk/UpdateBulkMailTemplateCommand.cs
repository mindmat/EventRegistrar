using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class UpdateBulkMailTemplateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public Guid TemplateId { get; set; }
    public string? Subject { get; set; }
    public string? ContentHtml { get; set; }
}

public class UpdateBulkMailTemplateCommandHandler : IRequestHandler<UpdateBulkMailTemplateCommand>
{
    private readonly IRepository<BulkMailTemplate> _bulkMailTemplates;
    private readonly IEventBus _eventBus;

    public UpdateBulkMailTemplateCommandHandler(IRepository<BulkMailTemplate> bulkMailTemplates,
                                                IEventBus eventBus)
    {
        _bulkMailTemplates = bulkMailTemplates;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UpdateBulkMailTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _bulkMailTemplates.AsTracking()
                                               .FirstAsync(mtp => mtp.EventId == command.EventId
                                                               && mtp.Id == command.TemplateId,
                                                           cancellationToken);
        template.Subject = command.Subject;
        if (!string.IsNullOrWhiteSpace(command.ContentHtml))
        {
            template.ContentHtml = command.ContentHtml;
        }

        //_eventBus.Publish(new QueryChanged
        //                  {
        //                      QueryName = nameof(BulkMailPreviewQuery),
        //                      EventId = command.EventId,
        //                      RowId = template.Id
        //                  });
        return Unit.Value;
    }
}