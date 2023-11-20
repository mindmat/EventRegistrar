using EventRegistrar.Backend.Infrastructure.DataAccess.ReadModels;
using EventRegistrar.Backend.Infrastructure.DomainEvents;

namespace EventRegistrar.Backend.Mailing.Bulk;

public class CreateBulkMailTemplateCommand : IRequest, IEventBoundRequest
{
    public Guid EventId { get; set; }
    public string? Key { get; set; }
}

public class CreateBulkMailTemplateCommandHandler(IRepository<BulkMailTemplate> templates,
                                                  MailConfiguration configuration,
                                                  IEventBus eventBus)
    : IRequestHandler<CreateBulkMailTemplateCommand>
{
    public async Task Handle(CreateBulkMailTemplateCommand command, CancellationToken cancellationToken)
    {
        if (command.Key == null)
        {
            throw new ArgumentNullException(nameof(command.Key));
        }

        var key = command.Key.ToLowerInvariant();

        var existingTemplates = await templates.Where(mtp => mtp.EventId == command.EventId
                                                          && mtp.BulkMailKey == key)
                                               .ToListAsync(cancellationToken);
        foreach (var language in configuration.AvailableLanguages)
        {
            var existingTemplate = existingTemplates.FirstOrDefault(btp => btp.Language == language);
            if (existingTemplate == null)
            {
                templates.InsertObjectTree(new BulkMailTemplate
                                           {
                                               Id = Guid.NewGuid(),
                                               EventId = command.EventId,
                                               BulkMailKey = key,
                                               Language = language
                                           });
            }
        }

        eventBus.Publish(new QueryChanged
                         {
                             EventId = command.EventId,
                             QueryName = nameof(BulkMailTemplatesQuery)
                         });
    }
}