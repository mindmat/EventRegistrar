using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class CreateBulkMailTemplateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? Key { get; set; }
}

public class CreateBulkMailTemplateCommandHandler : AsyncRequestHandler<CreateBulkMailTemplateCommand>
{
    private readonly IRepository<BulkMailTemplate> _templates;
    private readonly MailConfiguration _configuration;
    private readonly IEventBus _eventBus;

    public CreateBulkMailTemplateCommandHandler(IRepository<BulkMailTemplate> templates,
                                                MailConfiguration configuration,
                                                IEventBus eventBus)
    {
        _templates = templates;
        _configuration = configuration;
        _eventBus = eventBus;
    }

    protected override async Task Handle(CreateBulkMailTemplateCommand command, CancellationToken cancellationToken)
    {
        if (command.Key == null)
        {
            throw new ArgumentNullException(nameof(command.Key));
        }

        var key = command.Key.ToLowerInvariant();

        var existingTemplates = await _templates.Where(mtp => mtp.EventId == command.EventId
                                                           && mtp.BulkMailKey == key)
                                                .ToListAsync(cancellationToken);
        foreach (var language in _configuration.AvailableLanguages)
        {
            var existingTemplate = existingTemplates.FirstOrDefault(btp => btp.Language == language);
            if (existingTemplate == null)
            {
                _templates.InsertObjectTree(new BulkMailTemplate
                                            {
                                                Id = Guid.NewGuid(),
                                                EventId = command.EventId,
                                                BulkMailKey = key,
                                                Language = language
                                            });
            }
        }

        _eventBus.Publish(new QueryChanged
                          {
                              EventId = command.EventId,
                              QueryName = nameof(BulkMailTemplatesQuery)
                          });
    }
}