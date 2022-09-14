using EventRegistrar.Backend.Authorization;
using EventRegistrar.Backend.Infrastructure.DataAccess;
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

public class UpdateAutoMailTemplateCommandHandler : IRequestHandler<UpdateAutoMailTemplateCommand>
{
    private readonly IRepository<AutoMailTemplate> _autoMailTemplates;
    private readonly IEventBus _eventBus;

    public UpdateAutoMailTemplateCommandHandler(IRepository<AutoMailTemplate> autoMailTemplates,
                                                IEventBus eventBus)
    {
        _autoMailTemplates = autoMailTemplates;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(UpdateAutoMailTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _autoMailTemplates.AsTracking()
                                               .FirstAsync(mtp => mtp.EventId == command.EventId
                                                               && mtp.Id == command.TemplateId,
                                                           cancellationToken);
        template.Subject = command.Subject;
        if (!string.IsNullOrWhiteSpace(command.ContentHtml))
        {
            template.ContentHtml = command.ContentHtml;
        }

        _eventBus.Publish(new ReadModelUpdated
                          {
                              QueryName = nameof(AutoMailPreviewQuery),
                              EventId = command.EventId,
                              RowId = template.Id
                          });
        return Unit.Value;
    }
}